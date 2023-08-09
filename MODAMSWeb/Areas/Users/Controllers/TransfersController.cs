using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Telerik.SvgIcons;
using Barcode = MODAMS.Utility.Barcode;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class TransfersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;
        private int _storeId;
        public TransfersController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
        }
        public IActionResult Index(int id = 0, int transferStatusId = 0)
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;
            transferStatusId = 0;
            var stores = _db.Stores.ToList();
            var dto = new dtoTransfer();

            if (id == 0)
            {
                if (User.IsInRole("User") || User.IsInRole("StoreOwner"))
                {
                    _storeId = _func.GetStoreIdByEmployeeId(_employeeId);
                    var storeList = _db.Stores.ToList().Select(m => new SelectListItem
                    {
                        Text = m.Name,
                        Value = m.Id.ToString(),
                        Selected = (m.Id == _storeId)
                    });
                    dto.StoreList = storeList;
                }
                else
                {
                    var sl = _db.vwTransfers.Select(m => new { m.StoreFromId, m.StoreFrom }).Distinct().ToList();
                    int firstStoreId = sl.Select(m => m.StoreFromId).First();

                    var storeList = sl.Select(m => new SelectListItem
                    {
                        Text = m.StoreFrom,
                        Value = m.StoreFromId.ToString(),
                        Selected = (m.StoreFromId == firstStoreId)
                    });
                    dto.StoreList = storeList;
                    _storeId = firstStoreId;
                }
            }
            else
            {
                _storeId = id;
                var sl = _db.vwTransfers.Select(m => new { m.StoreFromId, m.StoreFrom }).Distinct().ToList();
                int firstStoreId = sl.Select(m => m.StoreFromId).First();

                var storeList = sl.Select(m => new SelectListItem
                {
                    Text = m.StoreFrom,
                    Value = m.StoreFromId.ToString(),
                    Selected = (m.StoreFromId == firstStoreId)
                });
                dto.StoreList = storeList;
            }
            if (_employeeId == _func.GetStoreOwnerId(_storeId))
                dto.IsAuthorized = true;

            var transfers = _db.vwTransfers.Where(m => m.StoreFromId == _storeId).ToList();

            if (transferStatusId > 0)
                transfers = transfers.Where(m => m.TransferStatusId == transferStatusId).ToList();

            TempData["transferStatus"] = transferStatusId;

            dto.StoreId = _storeId;
            dto.OutgoingTransfers = transfers;

            transfers = _db.vwTransfers.Where(m => m.StoreId == _storeId && m.TransferStatusId != SD.Transfer_Pending).ToList();
            dto.IncomingTransfers = transfers;

            return View(dto);
        }

        private string GetEmployeeStore(int employeeId)
        {
            return _func.GetDepartmentName(employeeId);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpGet]
        public IActionResult CreateTransfer()
        {
            var dto = new dtoCreateTransfer();
            List<MODAMS.Models.Asset> assets = GetAssets().GetAwaiter().GetResult();
            List<dtoTransferAsset> transferAssets = new List<dtoTransferAsset>();
            TempData["storeFrom"] = _func.GetDepartmentName(_employeeId);
            dto.Transfer.TransferNumber = GetTransferNumber();

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
            int currentStoreId = GetCurrentStoreId().GetAwaiter().GetResult();

            var storeList = _db.Stores.Where(m => m.Id != currentStoreId).ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString(),
            });

            dto.StoreList = storeList;

            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTransfer(dtoCreateTransfer transferDTO)
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;
            var storeId = _func.GetStoreIdByEmployeeId(_employeeId);



            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill all the mandatory fields";
                return RedirectToAction("CreateTransfer", "Transfers");
            }
            var transfer = new Transfer()
            {
                TransferDate = transferDTO.Transfer.TransferDate,
                EmployeeId = _employeeId,
                StoreFromId = storeId,
                StoreId = transferDTO.Transfer.StoreId,
                TransferNumber = transferDTO.Transfer.TransferNumber,
                TransferStatusId = 1,
                Notes = transferDTO.Transfer.Notes,
                SubmissionForAcknowledgementDate = DateTime.Now
            };

            try
            {
                _db.Transfers.Add(transfer);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("CreateTransfer", "Transfers");
            }

            var assets = transferDTO.Assets.Where(m => m.IsSelected == true).ToList();
            foreach (var asset in assets)
            {
                var transferDetail = new TransferDetail()
                {
                    AssetId = asset.AssetId,
                    PrevStoreId = GetCurrentStoreId().GetAwaiter().GetResult(),
                    TransferId = transfer.Id
                };
                _db.TransferDetails.Add(transferDetail);
            }
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("CreateTransfer", "Transfers");
            }

            TempData["success"] = "Transfer saved successfuly!";
            return RedirectToAction("EditTransfer", "Transfers", new { id = transfer.Id });
        }

        [Authorize(Roles = "StoreOwner, User")]
        public IActionResult EditTransfer(int id)
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;
            TempData["storeFrom"] = _func.GetDepartmentName(_employeeId);

            var transfer = _db.Transfers.Where(m => m.Id == id).FirstOrDefault();
            if (transfer == null)
            {
                TempData["error"] = "Transfer not found!";
                return RedirectToAction("Index", "Transfers");
            }
            var dto = new dtoEditTransfer();

            List<MODAMS.Models.Asset> assets = GetAssets().GetAwaiter().GetResult();
            List<dtoTransferAsset> transferAssets = new List<dtoTransferAsset>();

            dto.Transfer = transfer;
            var transferDetails = _db.TransferDetails.Where(m => m.TransferId == transfer.Id).ToList();

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
            int currentStoreId = GetCurrentStoreId().GetAwaiter().GetResult();

            var storeList = _db.Stores.Where(m => m.Id != currentStoreId).ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString(),
            });

            dto.StoreList = storeList;

            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTransfer(dtoEditTransfer transferDTO)
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;
            var storeId = _func.GetStoreIdByEmployeeId(_employeeId);

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill all the mandatory fields";
                return RedirectToAction("EditTransfer", "Transfers", new { id = transferDTO.Transfer.Id });
            }

            var transfer = _db.Transfers.Where(m => m.Id == transferDTO.Transfer.Id).FirstOrDefault();
            if (transfer == null)
            {
                TempData["error"] = "Record not found!";
                return RedirectToAction("EditTransfer", "Transfers", new { id = transferDTO.Transfer.Id });
            }

            try
            {
                transfer.TransferDate = transferDTO.Transfer.TransferDate;
                transfer.EmployeeId = _employeeId;
                transfer.StoreFromId = storeId;
                transfer.StoreId = transferDTO.Transfer.StoreId;
                transfer.TransferNumber = transferDTO.Transfer.TransferNumber;
                transfer.TransferStatusId = 1;
                transfer.Notes = transferDTO.Transfer.Notes;
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("EditTransfer", "Transfers", new { id = transferDTO.Transfer.Id });
            }
            var transferDetails = _db.TransferDetails.Where(m => m.TransferId == transferDTO.Transfer.Id).ToList();
            _db.RemoveRange(transferDetails);
            _db.SaveChanges();

            var assets = transferDTO.Assets.Where(m => m.IsSelected == true).ToList();
            foreach (var asset in assets)
            {
                var transferDetail = new TransferDetail()
                {
                    AssetId = asset.AssetId,
                    PrevStoreId = GetCurrentStoreId().GetAwaiter().GetResult(),
                    TransferId = transfer.Id
                };
                _db.TransferDetails.Add(transferDetail);
            }
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("EditTransfer", "Transfers", new { id = transferDTO.Transfer.Id });
            }

            TempData["success"] = "Transfer saved successfully!";
            return RedirectToAction("EditTransfer", "Transfers", new { id = transferDTO.Transfer.Id });
        }

        [Authorize(Roles = "StoreOwner, User")]
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

            dto.TransferBy = _func.GetEmployeeNameById(senderId);
            dto.ReceivedBy = _func.GetEmployeeNameById(receiverId);

            ViewBag.FromSignature = MODAMS.Utility.Barcode.GenerateBarCode(dto.TransferBy);
            ViewBag.ToSignature = MODAMS.Utility.Barcode.GenerateBarCode(dto.ReceivedBy);



            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitForAcknowledgement(int id)
        {
            var transfer = _db.Transfers.Where(m => m.Id == id).FirstOrDefault();
            if (transfer != null)
            {
                transfer.TransferStatusId = SD.Transfer_SubmittedForAcknowledgement;
                _db.SaveChanges();
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
        public IActionResult AcknowledgeTransfer(int id)
        {
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

                if (asset != null)
                {
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
                        _db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        TempData["error"] = ex.Message;
                        return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
                    }
                }
            }

            transfer.TransferStatusId = SD.Transfer_Completed;
            _db.SaveChanges();

            TempData["success"] = "Transfer Acknowledged";
            return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectTransfer(int id)
        {
            var transfer = _db.Transfers.Where(m => m.Id == id).FirstOrDefault();
            if (transfer != null)
            {
                transfer.TransferStatusId = SD.Transfer_Rejected;
                _db.SaveChanges();
                TempData["success"] = "Transfer Rejected";
                return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
            }
            else
            {
                TempData["error"] = "Record not found!";
                return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
            }
        }
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
        private string GetTransferNumber()
        {
            string sResult = "";

            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;

            if (User.IsInRole("User"))
                _employeeId = _func.GetSupervisorId(_employeeId);


            var transfers = _db.Transfers.Where(m => m.EmployeeId == _employeeId).ToList();
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

            return sResult;
        }
        private async Task<List<MODAMS.Models.Asset>> GetAssets()
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;

            //var assets = await _db.Assets.Include(m => m.Store.Department)
            //    .Include(m => m.SubCategory).Include(m => m.SubCategory.Category)
            //    .Where(m => m.AssetStatusId == 1).ToListAsync();
            //assets = assets.Where(m => m.Store.Department.EmployeeId == _employeeId).ToList();

            //var transferDetails = _db.TransferDetails.Include(m => m.Transfer).ToList();

            //transferDetails = transferDetails.Where(m => m.Transfer.TransferStatusId != SD.Transfer_Rejected).ToList();

            //assets = assets.Where(m => !transferDetails.Any(detail => detail.AssetId == m.Id)).ToList();

            var assets = await _db.Assets
                .Include(m => m.Store.Department)
                .Include(m => m.SubCategory.Category)
                .Where(m => m.AssetStatusId == 1 && m.Store.Department.EmployeeId == _employeeId)
                .ToListAsync();

            var transferDetails = await _db.TransferDetails
                .Include(m => m.Transfer)
                .Where(m => m.Transfer.TransferStatusId != SD.Transfer_Rejected)
                .ToListAsync();

            assets = assets
                .Where(asset => !transferDetails.Any(detail => detail.AssetId == asset.Id))
                .ToList();

            return assets;
        }
        private async Task<int> GetCurrentStoreId()
        {
            _employeeId = (User.IsInRole("User")) ? _func.GetSupervisorId(_employeeId) : _employeeId;

            int currentStoreId = await _db.Stores.Where(m => m.DepartmentId == _func.GetDepartmentId(_employeeId))
                .Select(m => m.Id).FirstOrDefaultAsync();

            return currentStoreId;
        }
    }
}
