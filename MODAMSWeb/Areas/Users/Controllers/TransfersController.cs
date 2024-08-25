using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using System.Runtime.CompilerServices;
using Notification = MODAMS.Models.Notification;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class TransfersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;
        private int _supervisorId;
        private int _storeId;
        public TransfersController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _supervisorId = _func.GetSupervisorId(_employeeId);
        }

        [HttpGet]
        public async Task<IActionResult> Index(int id = 0, int transferStatusId = 0)
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;
            transferStatusId = 0;
            var stores = await _db.Stores.ToListAsync();
            var dto = new dtoTransfer();


            if (id == 0)
            {
                if (User.IsInRole("User") || User.IsInRole("StoreOwner"))
                {
                    _storeId = _func.GetStoreIdByEmployeeId(_employeeId);
                    var storeList = stores.ToList().Select(m => new SelectListItem
                    {
                        Text = m.Name,
                        Value = m.Id.ToString(),
                        Selected = (m.Id == _storeId)
                    });
                    dto.StoreList = storeList;
                }
                else
                {
                    var sl = await _db.vwTransfers.Select(m => new { m.StoreFromId, m.StoreFrom }).Distinct().ToListAsync();

                    if (sl.Count > 0)
                    {
                        int firstStoreId = sl.Select(m => m.StoreFromId).FirstOrDefault();
                        var storeList = sl.Select(m => new SelectListItem
                        {
                            Text = m.StoreFrom,
                            Value = m.StoreFromId.ToString(),
                            Selected = (m.StoreFromId == firstStoreId)
                        });
                        dto.StoreList = storeList;
                        _storeId = firstStoreId;
                    }
                    else
                    {
                        int firstStoreId = sl.Select(m => m.StoreFromId).FirstOrDefault();
                        var storeList = sl.Select(m => new SelectListItem
                        {
                            Text = m.StoreFrom,
                            Value = m.StoreFromId.ToString(),
                            Selected = (m.StoreFromId == firstStoreId)
                        }).ToList();

                        if (!storeList.Any())
                        {
                            storeList.Add(new SelectListItem
                            {
                                Text = "No Transfer available",
                                Value = "0",
                                Selected = true // Select this item if there are no stores
                            });
                        }

                        dto.StoreList = storeList;
                        _storeId = firstStoreId;
                    }
                }
            }
            else
            {
                _storeId = id;
                var sl = await _db.vwTransfers.Select(m => new { m.StoreFromId, m.StoreFrom }).Distinct().ToListAsync();
                if (sl.Count > 0)
                {
                    int firstStoreId = sl.Select(m => m.StoreFromId).First();

                    var storeList = sl.Select(m => new SelectListItem
                    {
                        Text = m.StoreFrom,
                        Value = m.StoreFromId.ToString(),
                        Selected = (m.StoreFromId == firstStoreId)
                    });
                    dto.StoreList = storeList;
                }
            }
            if (_employeeId == _func.GetStoreOwnerId(_storeId))
                dto.IsAuthorized = true;

            var transfers = await _db.vwTransfers.Where(m => m.StoreFromId == _storeId)
                .OrderBy(m => m.TransferStatusId).ToListAsync();

            if (transferStatusId > 0)
                transfers = transfers.Where(m => m.TransferStatusId == transferStatusId).ToList();

            TempData["transferStatus"] = transferStatusId;

            dto.StoreId = _storeId;
            dto.OutgoingTransfers = transfers;

            transfers = await _db.vwTransfers
                .Where(m => m.StoreId == _storeId && m.TransferStatusId != SD.Transfer_Pending)
                .OrderBy(m => m.TransferStatusId)
                .ToListAsync();

            dto.IncomingTransfers = transfers;

            List<dtoTransferChart> outgoingChartData = await GetChartData(1);
            List<dtoTransferChart> incomingChartData = await GetChartData(2);

            dto.IncomingChartData = incomingChartData;
            dto.OutgoingChartData = outgoingChartData;

            dto.TotalTransferValue = await GetTotalTransferValue(_storeId);
            dto.TotalReceivedValue = await GetTotalReceivedValue(_storeId);

            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpGet]
        public async Task<IActionResult> CreateTransfer()
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;

            var dto = new dtoCreateTransfer();
            List<MODAMS.Models.Asset> assets = await GetAssets();
            List<dtoTransferAsset> transferAssets = new List<dtoTransferAsset>();


            TempData["storeFrom"] = await _func.GetDepartmentName(_employeeId);


            if (assets.Count > 0)
            {
                foreach (var asset in assets)
                {
                    var transferAsset = new dtoTransferAsset()
                    {
                        AssetId = asset.Id,
                        AssetName = asset.Name,
                        Category = asset.SubCategory.Category.CategoryName,
                        SubCategory = asset.SubCategory.SubCategoryName,
                        Make = asset.Make,
                        Model = asset.Model,
                        Barcode = asset.Barcode.ToString(),
                        SerialNumber = asset.SerialNo,
                        IsSelected = false
                    };
                    transferAssets.Add(transferAsset);
                }
            }
            if (transferAssets.Count > 0)
            {
                dto.Assets = transferAssets;
            }
            int currentStoreId = await GetCurrentStoreId();

            var storeList = _db.Stores.Where(m => m.Id != currentStoreId).ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString(),
            });
            dto.Transfer.TransferNumber = await GetTransferNumber(currentStoreId);
            dto.StoreList = storeList;

            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTransfer(dtoCreateTransfer transferDTO, string selectedAssets)
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;
            var storeId = _func.GetStoreIdByEmployeeId(_employeeId);


            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill all the mandatory fields";
                return RedirectToAction("CreateTransfer", "Transfers");
            }

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var transfer = new Transfer()
                    {
                        TransferDate = transferDTO.Transfer.TransferDate,
                        EmployeeId = _employeeId,
                        StoreFromId = storeId,
                        StoreId = transferDTO.Transfer.StoreId,
                        TransferNumber = transferDTO.Transfer.TransferNumber,
                        TransferStatusId = 1,
                        Notes = transferDTO.Transfer.Notes == null ? "-" : transferDTO.Transfer.Notes,
                        SubmissionForAcknowledgementDate = DateTime.Now
                    };
                    await _db.Transfers.AddAsync(transfer);
                    await _db.SaveChangesAsync();

                    string assetNamesForLog = "";
                    int prevStoreId = await GetCurrentStoreId();

                    if (!string.IsNullOrEmpty(selectedAssets))
                    {
                        var selectedIds = selectedAssets.Split(',').Select(id => int.Parse(id));
                        List<TransferDetail> transferDetails = new List<TransferDetail>();

                        foreach (var asset in selectedIds)
                        {
                            var transferDetail = new TransferDetail()
                            {
                                AssetId = asset,
                                PrevStoreId = prevStoreId,
                                TransferId = transfer.Id
                            };
                            transferDetails.Add(transferDetail);
                            assetNamesForLog += _func.GetAssetName(asset) + ", ";
                        }
                        _db.TransferDetails.AddRange(transferDetails);
                        await _db.SaveChangesAsync();
                    }

                    //Log NewsFeed
                    string employeeName = await _func.GetEmployeeName();
                    string assetName = assetNamesForLog;
                    string storefrom = _func.GetStoreNameByStoreId(prevStoreId);
                    string storeTo = _func.GetStoreNameByStoreId(transfer.StoreId);
                    string message = $"{employeeName} transferred ({assetName}) from {storefrom} to {storeTo}";
                    _func.LogNewsFeed(message, "Users", "Transfers", "PreviewTransfer", transfer.Id);

                    await _db.Database.CommitTransactionAsync();

                    //success
                    TempData["success"] = "Transfer saved successfuly!";
                    return RedirectToAction("PreviewTransfer", "Transfers", new { id = transfer.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["error"] = "Transaction failed: " + ex.Message;
                    return RedirectToAction("CreateTransfer", "Transfers");
                }
            }
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpGet]
        public async Task<IActionResult> EditTransfer(int id)
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;
            TempData["storeFrom"] = await _func.GetDepartmentName(_employeeId);

            var transfer = await _db.Transfers.FirstOrDefaultAsync(m => m.Id == id);
            if (transfer == null)
            {
                TempData["error"] = "Transfer not found!";
                return RedirectToAction("Index", "Transfers");
            }
            var dto = new dtoEditTransfer();

            List<MODAMS.Models.Asset> assets = await GetAssets();
            List<dtoTransferAsset> transferAssets = new List<dtoTransferAsset>();

            dto.Transfer = transfer;
            var transferDetails = await _db.TransferDetails.Where(m => m.TransferId == transfer.Id).ToListAsync();

            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    var transferAsset = new dtoTransferAsset()
                    {
                        AssetId = asset.Id,
                        AssetName = asset.Name,
                        Category = asset.SubCategory.Category.CategoryName,
                        SubCategory = asset.SubCategory.SubCategoryName,
                        Make = asset.Make,
                        Model = asset.Model,
                        Barcode = asset.Barcode.ToString(),
                        SerialNumber = asset.SerialNo,
                        IsSelected = IsAssetSelected(asset.Id, transferDetails)
                    };
                    transferAssets.Add(transferAsset);
                }
            }
            if (transferAssets.Count > 0)
            {
                dto.Assets = transferAssets.OrderByDescending(m => m.IsSelected).ToList();
            }
            int currentStoreId = await GetCurrentStoreId();

            var storeList = _db.Stores.Where(m => m.Id != currentStoreId).ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString(),
            });

            dto.StoreList = storeList;

            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> EditTransfer(dtoEditTransfer transferDTO, string selectedAssets)
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;
            var storeId = _func.GetStoreIdByEmployeeId(_employeeId);

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill all the mandatory fields";
                return RedirectToAction("EditTransfer", "Transfers", new { id = transferDTO.Transfer.Id });
            }

            var transfer = await _db.Transfers.Where(m => m.Id == transferDTO.Transfer.Id).FirstOrDefaultAsync();
            if (transfer == null)
            {
                TempData["error"] = "Record not found!";
                return RedirectToAction("EditTransfer", "Transfers", new { id = transferDTO.Transfer.Id });
            }
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    transfer.TransferDate = transferDTO.Transfer.TransferDate;
                    transfer.EmployeeId = _employeeId;
                    transfer.StoreFromId = storeId;
                    transfer.StoreId = transferDTO.Transfer.StoreId;
                    transfer.TransferNumber = transferDTO.Transfer.TransferNumber;
                    transfer.TransferStatusId = 1;
                    transfer.Notes = transferDTO.Transfer.Notes;
                    await _db.SaveChangesAsync();

                    var transferDetails = await _db.TransferDetails
                        .Where(m => m.TransferId == transferDTO.Transfer.Id).ToListAsync();

                    _db.RemoveRange(transferDetails);
                    await _db.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(selectedAssets))
                    {
                        var selectedIds = selectedAssets.Split(',').Select(id => int.Parse(id));
                        transferDetails = new List<TransferDetail>();

                        foreach (var asset in selectedIds)
                        {
                            var transferDetail = new TransferDetail()
                            {
                                AssetId = asset,
                                PrevStoreId = GetCurrentStoreId().GetAwaiter().GetResult(),
                                TransferId = transfer.Id
                            };
                            transferDetails.Add(transferDetail);
                        }
                        await _db.TransferDetails.AddRangeAsync(transferDetails);
                        await _db.SaveChangesAsync();
                    }
                    await _db.Database.CommitTransactionAsync();

                    //success
                    TempData["success"] = "Transfer saved successfuly!";
                    return RedirectToAction("PreviewTransfer", "Transfers", new { id = transfer.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["error"] = "Transaction failed: " + ex.Message;
                    return RedirectToAction("EditTransfer", "Transfers", new { id = transferDTO.Transfer.Id });
                }
            }
        }

        [HttpGet]
        public IActionResult PreviewTransfer(int id)
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;

            var transfer = _db.vwTransfers.Where(m => m.Id == id).FirstOrDefault();
            if (transfer == null)
                transfer = new vwTransfer();

            var dto = new dtoTransferPreview();
            dto.vwTransfer = transfer;

            var transferDetails = _db.TransferDetails.Where(m => m.TransferId == transfer.Id).ToList();
            var assets = _db.Assets.Include(m => m.SubCategory.Category).Include(m => m.Condition).Select(m => new
            {
                m.Id,
                m.Name,
                m.SubCategory.Category.CategoryName,
                m.SubCategory.SubCategoryName,
                m.Make,
                m.Model,
                m.Barcode,
                m.SerialNo,
                m.Condition.ConditionName,
                m.Plate
            }).ToList();

            var transferAssets = new List<dtoTransferAsset>();

            foreach (var transferDetail in transferDetails)
            {
                var asset = assets.Where(m => m.Id == transferDetail.AssetId).FirstOrDefault();
                if (asset == null)
                {
                    break;
                }
                var sIdentification = (asset.CategoryName == "Vehicles") ? "Plate No: " + asset.Plate.ToString() : "SN: " + asset.SerialNo.ToString();

                var transferAsset = new dtoTransferAsset()
                {
                    AssetId = asset.Id,
                    AssetName = asset.Name,
                    Category = asset.CategoryName,
                    SubCategory = asset.SubCategoryName,
                    Make = asset.Make,
                    Model = asset.Model,
                    Barcode = asset.Barcode.ToString(),
                    SerialNumber = sIdentification,
                    Condition = asset.ConditionName,
                    IsSelected = true
                };
                transferAssets.Add(transferAsset);
            }
            dto.transferAssets = transferAssets;

            int senderId = _func.GetStoreOwnerId(transfer.StoreFromId);
            int receiverId = _func.GetStoreOwnerId(transfer.StoreId);

            dto.IsSender = (senderId == _employeeId) ? true : false;
            dto.IsReceiver = (receiverId == _employeeId) ? true : false;

            dto.TransferBy = transfer.SenderBarcode;
            dto.ReceivedBy = transfer.ReceiverBarcode;

            if (transfer.SenderBarcode != "")
                ViewBag.FromSignature = MODAMS.Utility.Barcode.GenerateBarCode(dto.TransferBy);

            if (transfer.ReceiverBarcode != "")
                ViewBag.ToSignature = MODAMS.Utility.Barcode.GenerateBarCode(dto.ReceivedBy);

            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        public IActionResult DeleteTransfer(int id)
        {
            var transferToDelete = _db.Transfers.FirstOrDefault(m => m.Id == id);

            if (transferToDelete == null)
            {
                return RedirectToAction("Index", "Transfers");
            }

            try
            {
                _db.Transfers.Remove(transferToDelete);
                _db.SaveChanges();
                TempData["success"] = $"Transfer: {transferToDelete.TransferNumber} deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("EditTransfer", "Transfers", new { id });
            }

            return RedirectToAction("Index", "Transfers");
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForAcknowledgement(int id)
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;

            var transfer = _db.Transfers.Where(m => m.Id == id).FirstOrDefault();
            if (transfer != null)
            {
                int nEmployeeId = _func.GetEmployeeId();
                transfer.TransferStatusId = SD.Transfer_SubmittedForAcknowledgement;
                transfer.SenderBarcode = await _func.GetEmployeeName(nEmployeeId);

                await _db.SaveChangesAsync();

                Notification notification = new Notification()
                {
                    EmployeeFrom = _employeeId,
                    EmployeeTo = _func.GetStoreOwnerId(transfer.StoreId),
                    Subject = "Transfer awaiting acknowledgement",
                    Message = "A new transfer has been submitted by "
                       + _func.GetEmployeeNameById(_employeeId) +
                       " for your acknowledgement, please click the following link and follow the instructions",
                    DateTime = DateTime.Now,
                    IsViewed = false,
                    TargetRecordId = transfer.Id,
                    NotificationSectionId = SD.NS_Transfer
                };
                int departmentId = await _func.GetDepartmentId(notification.EmployeeTo);

                _func.NotifyDepartment(departmentId, notification);

                //Log NewsFeed
                string employeeName = await _func.GetEmployeeName();
                string message = $"{employeeName} submitted the transfer ({transfer.TransferNumber}) for acknowledgement";
                _func.LogNewsFeed(message, "Users", "Transfers", "PreviewTransfer", transfer.Id);


                TempData["success"] = "Transfer Submitted for Acknowledgement";
                return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });

            }
            else
            {
                TempData["error"] = "Record not found!";
                return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
            }
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcknowledgeTransfer(int id)
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;

            var transfer = _db.Transfers.FirstOrDefault(m => m.Id == id);

            if (transfer == null)
            {
                TempData["error"] = "Record not found!";
                return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
            }

            var transferDetails = _db.TransferDetails
                .Where(m => m.TransferId == id)
                .ToList();

            foreach (var item in transferDetails)
            {
                var asset = _db.Assets.FirstOrDefault(m => m.Id == item.AssetId);

                if (asset == null)
                    continue;
                try
                {
                    var fromStoreName = _func.GetStoreNameByStoreId(item.Transfer.StoreFromId);
                    var toStoreName = _func.GetStoreNameByStoreId(item.Transfer.StoreId);

                    var assetHistory = new AssetHistory()
                    {
                        AssetId = item.AssetId,
                        Description = $"Asset Transferred from {fromStoreName} to {toStoreName}",
                        TimeStamp = DateTime.Now,
                        TransactionRecordId = item.TransferId,
                        TransactionTypeId = SD.Transaction_Transfer
                    };

                    _db.AssetHistory.Add(assetHistory);
                    asset.StoreId = item.Transfer.StoreId;
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    TempData["error"] = ex.Message;
                    return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
                }
            }
            int nEmployeeId = _func.GetEmployeeId();

            transfer.TransferStatusId = SD.Transfer_Completed;
            transfer.ReceiverBarcode = await _func.GetEmployeeName(nEmployeeId);
            transfer.AcknowledgementDate = DateTime.Now;

            await _db.SaveChangesAsync();

            var ownerId = _func.GetStoreOwnerId(transfer.StoreFromId);
            var employeeName = _func.GetEmployeeNameById(_employeeId);

            var notification = new Notification()
            {
                EmployeeFrom = _employeeId,
                EmployeeTo = ownerId,
                Subject = "Transfer acknowledged",
                Message = $"Transfer Number: <b>{transfer.TransferNumber}</b> has been acknowledged by {employeeName}, please click the following link for details",
                DateTime = DateTime.Now,
                IsViewed = false,
                TargetRecordId = transfer.Id,
                NotificationSectionId = SD.NS_Transfer
            };

            var departmentId = await _func.GetDepartmentId(notification.EmployeeTo);
            _func.NotifyDepartment(departmentId, notification);


            //Log NewsFeed
            string message = $"{employeeName} acknowledged the transfer ({transfer.TransferNumber})";
            _func.LogNewsFeed(message, "Users", "Transfers", "PreviewTransfer", transfer.Id);


            TempData["success"] = "Transfer Acknowledged";
            return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectTransfer(int id)
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;
            var transfer = _db.Transfers.FirstOrDefault(m => m.Id == id);

            if (transfer == null)
            {
                TempData["error"] = "Record not found!";
            }
            else
            {
                transfer.TransferStatusId = SD.Transfer_Rejected;
                transfer.AcknowledgementDate = DateTime.Now;
                transfer.ReceiverBarcode = await _func.GetEmployeeName(_employeeId);

                await _db.SaveChangesAsync();

                var ownerId = _func.GetStoreOwnerId(transfer.StoreFromId);
                var employeeName = _func.GetEmployeeNameById(_employeeId);

                var notification = new Notification
                {
                    EmployeeFrom = _employeeId,
                    EmployeeTo = ownerId,
                    Subject = "Transfer rejected",
                    Message = $"Transfer Number: <b>{transfer.TransferNumber}</b> has been rejected by {employeeName}, please click the following link for details",
                    DateTime = DateTime.Now,
                    IsViewed = false,
                    TargetRecordId = transfer.Id,
                    NotificationSectionId = SD.NS_Transfer
                };

                var departmentId = await _func.GetDepartmentId(notification.EmployeeTo);
                _func.NotifyDepartment(departmentId, notification);

                //Log NewsFeed
                string message = $"{employeeName} rejected the transfer ({transfer.TransferNumber})";
                _func.LogNewsFeed(message, "Users", "Transfers", "PreviewTransfer", transfer.Id);

                TempData["success"] = "Transfer Rejected";
            }

            return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
        }
        public async Task<List<dtoTransferChart>> GetChartData(int type)
        {
            List<dtoTransferChart> dtoTransferCharts = new List<dtoTransferChart>();

            var result = await _db.SubCategories
            .Join(_db.Assets, subCategory => subCategory.Id, asset => asset.SubCategoryId, (subCategory, asset) => new { subCategory, asset })
            .Join(_db.TransferDetails, combined => combined.asset.Id, transferDetail => transferDetail.AssetId, (combined, transferDetail) => new { combined.subCategory, combined.asset, transferDetail })
            .Join(_db.Transfers, combined => combined.transferDetail.TransferId, transfer => transfer.Id, (combined, transfer) => new { combined.subCategory, combined.asset, combined.transferDetail, transfer })
            .Where(combined => combined.transfer.TransferStatusId == 3)
            .GroupBy(combined => new
            {
                combined.subCategory.Id,
                combined.subCategory.SubCategoryName,
                combined.transfer.StoreFromId,
                combined.transfer.StoreId
            })
            .Select(grouped => new
            {
                Id = grouped.Key.Id,
                SubCategoryName = grouped.Key.SubCategoryName,
                TotalAssets = grouped.Count(),
                StoreFromId = grouped.Key.StoreFromId,
                StoreToId = grouped.Key.StoreId
            })
            .ToListAsync();

            foreach (var item in result)
            {
                dtoTransferChart dto = new dtoTransferChart()
                {
                    Id = item.Id,
                    SubCategoryName = item.SubCategoryName,
                    TotalAssets = item.TotalAssets,
                    StoreFromId = item.StoreFromId,
                    StoreToId = item.StoreToId
                };
                dtoTransferCharts.Add(dto);
            }

            if (type == 1)
            {
                dtoTransferCharts = dtoTransferCharts.Where(m => m.StoreFromId == _storeId).ToList();
            }
            else
            {
                dtoTransferCharts = dtoTransferCharts.Where(m => m.StoreToId == _storeId).ToList();
            }

            return dtoTransferCharts;
        }

        [HttpGet]
        public IActionResult TransferredAssets(int id)
        {
            var transferDetails = _db.TransferDetails
                .Include(td => td.Transfer)
                .Where(td => td.Transfer.StoreFromId == id && td.Transfer.TransferStatusId == 3)
                .ToList();

            var assetIds = transferDetails.Select(td => td.AssetId).ToList();

            var assets = _db.Assets
                .Include(asset => asset.SubCategory.Category)
                .Where(asset => assetIds.Contains(asset.Id))
                .ToList();

            var dto = new List<dtoTransfersOutgoingAsset>();

            foreach (var td in transferDetails)
            {
                var transferAsset = assets.FirstOrDefault(a => a.Id == td.AssetId);

                if (transferAsset != null)
                {
                    var outgoingAsset = new dtoTransfersOutgoingAsset
                    {
                        TransferDate = td.Transfer.TransferDate,
                        StoreFrom = _func.GetStoreNameByStoreId(td.Transfer.StoreFromId),
                        StoreTo = _func.GetStoreNameByStoreId(td.Transfer.StoreId),
                        AssetId = td.AssetId,
                        Category = transferAsset.SubCategory?.Category?.CategoryName,
                        SubCategory = transferAsset.SubCategory?.SubCategoryName,
                        Make = transferAsset.Make,
                        Model = transferAsset.Model,
                        AssetName = transferAsset.Name,
                        Barcode = transferAsset.Barcode,
                        Condition = transferAsset.Condition?.ToString(),
                        SerialNumber = transferAsset.SerialNo,
                        Cost = transferAsset.Cost,
                        CurrentValue = _func.GetDepreciatedCost(td.AssetId)
                    };

                    dto.Add(outgoingAsset);
                }
            }

            return View(dto);
        }

        [HttpGet]
        public IActionResult ReceivedAssets(int id)
        {
            var transferDetails = _db.TransferDetails
                .Include(td => td.Transfer)
                .Where(td => td.Transfer.StoreId == id && td.Transfer.TransferStatusId == 3)
                .ToList();

            var assetIds = transferDetails.Select(td => td.AssetId).ToList();

            var assets = _db.Assets
                .Include(asset => asset.SubCategory.Category)
                .Where(asset => assetIds.Contains(asset.Id))
                .ToList();

            var dto = new List<dtoTransfersIncomingAsset>();

            foreach (var td in transferDetails)
            {
                var transferAsset = assets.FirstOrDefault(a => a.Id == td.AssetId);

                if (transferAsset != null)
                {
                    var incomingAssets = new dtoTransfersIncomingAsset
                    {
                        TransferDate = td.Transfer.TransferDate,
                        StoreFrom = _func.GetStoreNameByStoreId(td.Transfer.StoreFromId),
                        StoreTo = _func.GetStoreNameByStoreId(td.Transfer.StoreId),
                        AssetId = td.AssetId,
                        Category = transferAsset.SubCategory?.Category?.CategoryName,
                        SubCategory = transferAsset.SubCategory?.SubCategoryName,
                        Make = transferAsset.Make,
                        Model = transferAsset.Model,
                        AssetName = transferAsset.Name,
                        Barcode = transferAsset.Barcode,
                        Condition = transferAsset.Condition?.ToString(),
                        SerialNumber = transferAsset.SerialNo,
                        Cost = transferAsset.Cost,
                        CurrentValue = _func.GetDepreciatedCost(td.AssetId)
                    };

                    dto.Add(incomingAssets);
                }
            }

            return View(dto);
        }

        [HttpPost]
        public IActionResult PrintVoucher(int id)
        {
            return RedirectToAction("ReportViewerExternal", "Reporting", new { Type = "PrintVoucher", id = id }); // Redirect to a different action after processing the form
        }


        //Private functions
        private bool IsAssetSelected(int assetId, List<TransferDetail> transferDetails)
        {
            bool blnResult = false;
            var td = transferDetails.Where(m => m.AssetId == assetId).FirstOrDefault();
            if (td != null)
            {
                blnResult = true;
            }
            return blnResult;
        }
        private async Task<string> GetTransferNumber(int currentStoreId)
        {
            string sResult = "";

            var transfers = await _db.Transfers.Where(m => m.EmployeeId == _employeeId).ToListAsync();
            var maxIdTransfer = transfers.OrderByDescending(m => m.Id).FirstOrDefault();

            int maxId = 0;
            if (maxIdTransfer != null)
            {
                maxId = maxIdTransfer.Id;
            };
            maxId++;

            if (maxId < 10)
            {
                sResult = "0000" + maxId;
            }
            else if (maxId > 10 && maxId < 100)
            {
                sResult = "000" + maxId;
            }
            else if (maxId > 100 && maxId < 1000)
            {
                sResult = "00" + maxId;
            }
            else if (maxId > 1000 && maxId < 10000)
            {
                sResult = "0" + maxId;
            }
            else if (maxId > 10000 && maxId < 100000)
            {
                sResult = maxId.ToString();
            }
            else
            {
                sResult = maxId.ToString();
            }

            return currentStoreId + "-" + sResult;
        }
        private async Task<List<MODAMS.Models.Asset>> GetAssets([CallerMemberName] string caller = "")
        {
            var assets = await _db.Assets
                .Include(m => m.Store.Department)
                .Include(m => m.SubCategory.Category)
                .Where(m => m.AssetStatusId == 1 && m.Store.Department.EmployeeId == _employeeId)
                .ToListAsync();

            var transferDetails = await _db.TransferDetails
                .Include(m => m.Transfer)
                .Where(m => m.Transfer.TransferStatusId == SD.Transfer_Pending ||
                m.Transfer.TransferStatusId == SD.Transfer_SubmittedForAcknowledgement)
                .ToListAsync();
            if (caller == "CreateTransfer")
            {
                assets = assets
                .Where(m => !transferDetails.Any(detail => detail.AssetId == m.Id))
                .ToList();
            }


            return assets;
        }
        private async Task<int> GetCurrentStoreId()
        {
            int departmentId = await _func.GetDepartmentId(_employeeId);
            return _func.GetStoreIdByDepartmentId(departmentId);

        }
        private async Task<decimal> GetTotalTransferValue(int storeId)
        {
            var transferIds = await _db.Transfers
                .Where(transfer => transfer.TransferStatusId == 3 && transfer.StoreFromId == storeId)
                .Select(transfer => transfer.Id)
                .ToListAsync();

            var transferDetails = await _db.TransferDetails.ToListAsync();
            decimal totalValue = 0;

            foreach (var transferId in transferIds)
            {
                var transferDetailValue = transferDetails
                    .Where(detail => detail.TransferId == transferId)
                    .Sum(detail => _func.GetDepreciatedCost(detail.AssetId));

                totalValue += transferDetailValue;
            }

            return totalValue;
        }
        private async Task<decimal> GetTotalReceivedValue(int storeId)
        {
            var transferIds = await _db.Transfers
                .Where(transfer => transfer.TransferStatusId == 3 && transfer.StoreId == storeId)
                .Select(transfer => transfer.Id)
                .ToListAsync();

            var transferDetails = await _db.TransferDetails.ToListAsync();
            decimal totalValue = 0;

            foreach (var transferId in transferIds)
            {
                var transferDetailValue = transferDetails
                    .Where(detail => detail.TransferId == transferId)
                    .Sum(detail => _func.GetDepreciatedCost(detail.AssetId));

                totalValue += transferDetailValue;
            }

            return totalValue;
        }
    }
}
