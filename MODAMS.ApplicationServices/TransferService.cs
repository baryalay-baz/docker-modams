using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MODAMS.ApplicationServices.IServices;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices
{
    public class TransferService : ITransferService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly ILogger<TransferService> _logger;

        private int _employeeId;
        private int _supervisorId;
        private int _storeId;
        private readonly bool _isSomali;

        public TransferService(ApplicationDbContext db, IAMSFunc func, IHttpContextAccessor httpContextAccessor, ILogger<TransferService> logger)
        {
            _db = db;
            _func = func;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            _employeeId = _func.GetEmployeeId();
            _supervisorId = _func.GetSupervisorId(_employeeId);
            _isSomali = CultureInfo.CurrentUICulture.Name == "so";
        }
        private bool IsInRole(string role) => _httpContextAccessor.HttpContext.User.IsInRole(role);
        public async Task<Result<TransferDTO>> GetIndexAsync(int id = 0, int transferStatusId = 0)
        {
            try
            {
                _employeeId = (IsInRole("User")) ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
                transferStatusId = 0;
                var stores = await _db.Stores.ToListAsync();
                var dto = new TransferDTO();

                if (id == 0)
                {
                    if (IsInRole("User") || IsInRole("StoreOwner"))
                    {
                        _storeId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);
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
                if (_employeeId == await _func.GetStoreOwnerIdAsync(_storeId))
                    dto.IsAuthorized = true;

                var transfers = await _db.vwTransfers.Where(m => m.StoreFromId == _storeId)
                    .OrderBy(m => m.TransferStatusId).ToListAsync();

                if (transferStatusId > 0)
                    transfers = transfers.Where(m => m.TransferStatusId == transferStatusId).ToList();

                dto.TransferStatus = transferStatusId;

                dto.StoreId = _storeId;
                dto.OutgoingTransfers = transfers;

                transfers = await _db.vwTransfers
                    .Where(m => m.StoreId == _storeId && m.TransferStatusId != SD.Transfer_Pending)
                    .OrderBy(m => m.TransferStatusId)
                    .ToListAsync();

                dto.IncomingTransfers = transfers;

                List<TransferChartDTO> outgoingChartData = await GetChartDataAsync(1);
                List<TransferChartDTO> incomingChartData = await GetChartDataAsync(2);

                dto.IncomingChartData = incomingChartData;
                dto.OutgoingChartData = outgoingChartData;

                dto.TotalTransferValue = await GetTotalTransferValueAsync(_storeId);
                dto.TotalReceivedValue = await GetTotalReceivedValueAsync(_storeId);

                return Result<TransferDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<TransferDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<TransferCreateDTO>> GetCreateTransferAsync()
        {
            try
            {
                _employeeId = (IsInRole("User")) ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;

                var dto = new TransferCreateDTO();
                List<MODAMS.Models.Asset> assets = await GetAssetsAsync();
                List<TransferAssetDTO> transferAssets = new List<TransferAssetDTO>();


                dto.StoreFrom = await _func.GetDepartmentNameAsync(_employeeId);

                if (assets.Count > 0)
                {
                    foreach (var asset in assets)
                    {
                        var transferAsset = new TransferAssetDTO()
                        {
                            AssetId = asset.Id,
                            AssetName = asset.Name,
                            Category = _isSomali ? asset.SubCategory.Category.CategoryNameSo : asset.SubCategory.Category.CategoryName,
                            SubCategory = _isSomali ? asset.SubCategory.SubCategoryNameSo : asset.SubCategory.SubCategoryName,
                            Make = asset.Make,
                            Model = asset.Model,
                            Barcode = asset.Barcode?.ToString() ?? "",
                            SerialNumber = asset.SerialNo,
                            ImageUrl = await GetAssetImageAsync(asset.Id),
                            IsSelected = false
                        };
                        transferAssets.Add(transferAsset);
                    }
                }
                if (transferAssets.Count > 0)
                {
                    dto.Assets = transferAssets;
                }
                int currentStoreId = await GetCurrentStoreIdAsync();

                var storeList = await _db.Stores.Where(m => m.Id != currentStoreId).Select(m => new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString(),
                }).ToListAsync();

                dto.Transfer.TransferNumber = await GetTransferNumberAsync(currentStoreId);
                dto.StoreList = storeList;

                return Result<TransferCreateDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<TransferCreateDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<TransferCreateDTO>> CreateTransferAsync(TransferCreateDTO transferDTO, string selectedAssets)
        {
            try
            {
                _employeeId = (IsInRole("User")) ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
                var storeId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);

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
                        int prevStoreId = await GetCurrentStoreIdAsync();

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
                                assetNamesForLog += await _func.GetAssetNameAsync(asset) + ", ";
                            }
                            _db.TransferDetails.AddRange(transferDetails);
                            await _db.SaveChangesAsync();
                        }

                        //Log NewsFeed
                        string employeeName = await _func.GetEmployeeNameAsync();
                        string assetName = assetNamesForLog;
                        string storefrom = await _func.GetStoreNameByStoreIdAsync(prevStoreId);
                        string storeTo = await _func.GetStoreNameByStoreIdAsync(transfer.StoreId);
                        string message = $"{employeeName} transferred ({assetName}) from {storefrom} to {storeTo}";
                        await _func.LogNewsFeedAsync(message, "Users", "Transfers", "PreviewTransfer", transfer.Id);

                        await _db.Database.CommitTransactionAsync();

                        transferDTO.Transfer.Id = transfer.Id;

                        //success
                        return Result<TransferCreateDTO>.Success(transferDTO);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _func.LogException(_logger, ex);
                        return Result<TransferCreateDTO>.Failure(ex.Message, transferDTO);
                    }
                }

            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<TransferCreateDTO>.Failure(ex.Message, transferDTO);
            }
        }
        public async Task<Result<TransferEditDTO>> GetEditTransferAsync(int transferId)
        {
            try
            {
                _employeeId = (IsInRole("User")) ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;


                var transfer = await _db.Transfers.FirstOrDefaultAsync(m => m.Id == transferId);
                if (transfer == null)
                {
                    return Result<TransferEditDTO>.Failure("Transfer not found!");
                }
                var dto = new TransferEditDTO();

                dto.StoreFrom = await _func.GetDepartmentNameAsync(_employeeId);

                List<MODAMS.Models.Asset> assets = await GetAssetsAsync();
                List<TransferAssetDTO> transferAssets = new List<TransferAssetDTO>();

                dto.Transfer = transfer;
                var transferDetails = await _db.TransferDetails.Where(m => m.TransferId == transfer.Id).ToListAsync();

                if (assets != null)
                {
                    foreach (var asset in assets)
                    {
                        var transferAsset = new TransferAssetDTO()
                        {
                            AssetId = asset.Id,
                            AssetName = asset.Name,
                            Category = _isSomali ? asset.SubCategory.Category.CategoryNameSo : asset.SubCategory.Category.CategoryName,
                            SubCategory = _isSomali ? asset.SubCategory.SubCategoryNameSo : asset.SubCategory.SubCategoryName,
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
                int currentStoreId = await GetCurrentStoreIdAsync();

                var storeList = _db.Stores.Where(m => m.Id != currentStoreId).ToList().Select(m => new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString(),
                });

                dto.StoreList = storeList;

                return Result<TransferEditDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<TransferEditDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<TransferEditDTO>> EditTransferAsync(TransferEditDTO transferDTO, string selectedAssets)
        {
            try
            {
                _employeeId = IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
                var storeId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);

                var transfer = await _db.Transfers.Where(m => m.Id == transferDTO.Transfer.Id).FirstOrDefaultAsync();
                if (transfer == null)
                {
                    return Result<TransferEditDTO>.Failure("Record not found!");
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
                                    PrevStoreId = await GetCurrentStoreIdAsync(),
                                    TransferId = transfer.Id
                                };
                                transferDetails.Add(transferDetail);
                            }
                            await _db.TransferDetails.AddRangeAsync(transferDetails);
                            await _db.SaveChangesAsync();
                        }
                        await _db.Database.CommitTransactionAsync();

                        //success
                        return Result<TransferEditDTO>.Success(transferDTO);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _func.LogException(_logger, ex);
                        return Result<TransferEditDTO>.Failure(ex.Message, transferDTO);
                    }
                }
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<TransferEditDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<TransferPreviewDTO>> GetPreviewTransferAsync(int transferId)
        {
            try
            {
                _employeeId = (IsInRole("User")) ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;

                var transfer = _db.vwTransfers.Where(m => m.Id == transferId).FirstOrDefault();
                if (transfer == null)
                    transfer = new vwTransfer();

                var dto = new TransferPreviewDTO();
                dto.vwTransfer = transfer;

                var transferDetails = await _db.TransferDetails.Where(m => m.TransferId == transfer.Id).ToListAsync();
                var assets = await _db.Assets.Include(m => m.SubCategory.Category).Include(m => m.Condition).Select(m => new
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
                }).ToListAsync();

                var transferAssets = new List<TransferAssetDTO>();

                foreach (var transferDetail in transferDetails)
                {
                    var asset = assets.Where(m => m.Id == transferDetail.AssetId).FirstOrDefault();
                    if (asset == null)
                    {
                        break;
                    }
                    var sIdentification = (asset.CategoryName == "Vehicles") ? "Plate No: " + asset.Plate.ToString() : "SN: " + asset.SerialNo.ToString();

                    var transferAsset = new TransferAssetDTO()
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

                int senderId = await _func.GetStoreOwnerIdAsync(transfer.StoreFromId);
                int receiverId = await _func.GetStoreOwnerIdAsync(transfer.StoreId);

                dto.IsSender = (senderId == _employeeId) ? true : false;
                dto.IsReceiver = (receiverId == _employeeId) ? true : false;

                dto.TransferBy = transfer.SenderBarcode;
                dto.ReceivedBy = transfer.ReceiverBarcode;

                if (transfer.SenderBarcode != "")
                    dto.FromSignature = MODAMS.Utility.Barcode.GenerateBarCode(dto.TransferBy);

                if (transfer.ReceiverBarcode != "")
                    dto.ToSignature = MODAMS.Utility.Barcode.GenerateBarCode(dto.ReceivedBy);

                return Result<TransferPreviewDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<TransferPreviewDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<string>> DeleteTransferAsync(int transferId)
        {
            try
            {
                var transferToDelete = await _db.Transfers.FirstOrDefaultAsync(m => m.Id == transferId);

                if (transferToDelete == null)
                {
                    return Result<string>.Failure("Transfer record not found!");
                }
                try
                {
                    _db.Transfers.Remove(transferToDelete);
                    _db.SaveChanges();
                    return Result<string>.Success(transferToDelete.TransferNumber);
                }
                catch (Exception ex)
                {
                    _func.LogException(_logger, ex);
                    return Result<string>.Failure(ex.Message);
                }
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<string>.Failure(ex.Message);
            }
        }
        public async Task<Result> SubmitForAcknowledgementAsync(int transferId)
        {
            try
            {
                _employeeId = (IsInRole("User")) ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;

                var transfer = await _db.Transfers.Where(m => m.Id == transferId).FirstOrDefaultAsync();

                if (transfer == null)
                {
                    return Result.Failure("Transfer not found!");
                }

                int nEmployeeId = await _func.GetEmployeeIdAsync();
                transfer.TransferStatusId = SD.Transfer_SubmittedForAcknowledgement;
                transfer.SenderBarcode = await _func.GetEmployeeNameAsync(nEmployeeId);

                await _db.SaveChangesAsync();

                Notification notification = new Notification()
                {
                    EmployeeFrom = _employeeId,
                    EmployeeTo = await _func.GetStoreOwnerIdAsync(transfer.StoreId),
                    Subject = "Transfer awaiting acknowledgement",
                    Message = "A new transfer has been submitted by "
                       + await _func.GetEmployeeNameByIdAsync(_employeeId) +
                       " for your acknowledgement, please click the following link and follow the instructions",
                    DateTime = DateTime.Now,
                    IsViewed = false,
                    TargetRecordId = transfer.Id,
                    NotificationSectionId = SD.NS_Transfer
                };
                int departmentId = await _func.GetDepartmentIdAsync(notification.EmployeeTo);

                await _func.NotifyDepartmentAsync(departmentId, notification);

                //Log NewsFeed
                string employeeName = await _func.GetEmployeeNameAsync();
                string message = $"{employeeName} submitted the transfer ({transfer.TransferNumber}) for acknowledgement";
                await _func.LogNewsFeedAsync(message, "Users", "Transfers", "PreviewTransfer", transfer.Id);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result.Failure(ex.Message);
            }
        }
        public async Task<Result> AcknowledgeTransferAsync(int transferId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                _employeeId = IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;

                var transfer = await _db.Transfers.FirstOrDefaultAsync(m => m.Id == transferId);

                if (transfer == null)
                {
                    return Result.Failure("Transfer not found!");
                }

                var transferDetails = await _db.TransferDetails
                    .Where(m => m.TransferId == transferId)
                    .ToListAsync();

                foreach (var item in transferDetails)
                {
                    var asset = await _db.Assets.FirstOrDefaultAsync(m => m.Id == item.AssetId);
                    if (asset == null)
                        continue;

                    var fromStoreName = await _func.GetStoreNameByStoreIdAsync(transfer.StoreFromId);
                    var toStoreName = await _func.GetStoreNameByStoreIdAsync(transfer.StoreId);

                    var assetHistory = new AssetHistory
                    {
                        AssetId = item.AssetId,
                        Description = $"Asset Transferred from {fromStoreName} to {toStoreName}",
                        TimeStamp = DateTime.Now,
                        TransactionRecordId = item.TransferId,
                        TransactionTypeId = SD.Transaction_Transfer
                    };

                    _db.AssetHistory.Add(assetHistory);

                    // Update the asset's current store
                    asset.StoreId = transfer.StoreId;
                }

                await _db.SaveChangesAsync();

                // Update transfer status
                transfer.TransferStatusId = SD.Transfer_Completed;
                transfer.ReceiverBarcode = await _func.GetEmployeeNameAsync(_employeeId);
                transfer.AcknowledgementDate = DateTime.Now;

                await _db.SaveChangesAsync();

                var ownerId = await _func.GetStoreOwnerIdAsync(transfer.StoreFromId);
                var employeeName = await _func.GetEmployeeNameByIdAsync(_employeeId);

                var notification = new Notification
                {
                    EmployeeFrom = _employeeId,
                    EmployeeTo = ownerId,
                    Subject = "Transfer acknowledged",
                    Message = $"Transfer Number: <b>{transfer.TransferNumber}</b> has been acknowledged by {employeeName}.",
                    DateTime = DateTime.Now,
                    IsViewed = false,
                    TargetRecordId = transfer.Id,
                    NotificationSectionId = SD.NS_Transfer
                };

                var departmentId = await _func.GetDepartmentIdAsync(notification.EmployeeTo);
                await _func.NotifyDepartmentAsync(departmentId, notification);

                string message = $"{employeeName} acknowledged the transfer ({transfer.TransferNumber})";
                await _func.LogNewsFeedAsync(message, "Users", "Transfers", "PreviewTransfer", transfer.Id);

                await transaction.CommitAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _func.LogException(_logger, ex);
                return Result.Failure($"Error while acknowledging transfer: {ex.Message}");
            }
        }
        public async Task<Result> RejectTransferAsync(int transferId, string txtReason = "")
        {
            try
            {
                _employeeId = (IsInRole("User")) ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
                var transfer = _db.Transfers.FirstOrDefault(m => m.Id == transferId);

                if (transfer == null)
                    return Result.Failure("Transfer record not found!");


                transfer.TransferStatusId = SD.Transfer_Rejected;
                transfer.AcknowledgementDate = DateTime.Now;
                transfer.ReceiverBarcode = await _func.GetEmployeeNameAsync(_employeeId);

                await _db.SaveChangesAsync();

                var ownerId = await _func.GetStoreOwnerIdAsync(transfer.StoreFromId);
                var employeeName = await _func.GetEmployeeNameByIdAsync(_employeeId);

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

                var departmentId = await _func.GetDepartmentIdAsync(notification.EmployeeTo);
                await _func.NotifyDepartmentAsync(departmentId, notification);

                //Log NewsFeed
                string message = $"{employeeName} rejected the transfer ({transfer.TransferNumber})";
                await _func.LogNewsFeedAsync(message, "Users", "Transfers", "PreviewTransfer", transfer.Id);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result.Failure(ex.Message);
            }
        }
        public async Task<Result<List<TransfersOutgoingAssetDTO>>> GetTransferredAssetsAsync(int storeId)
        {
            try
            {
                var transferDetails = await _db.TransferDetails
                .Include(td => td.Transfer)
                .Where(td => td.Transfer.StoreFromId == storeId && td.Transfer.TransferStatusId == 3)
                .ToListAsync();

                var assetIds = transferDetails.Select(td => td.AssetId).ToList();

                var assets = await _db.Assets
                    .Include(asset => asset.SubCategory.Category)
                    .Where(asset => assetIds.Contains(asset.Id))
                    .ToListAsync();

                var dto = new List<TransfersOutgoingAssetDTO>();

                foreach (var td in transferDetails)
                {
                    var transferAsset = assets.FirstOrDefault(a => a.Id == td.AssetId);

                    if (transferAsset != null)
                    {
                        var outgoingAsset = new TransfersOutgoingAssetDTO
                        {
                            TransferDate = td.Transfer.TransferDate,
                            StoreFrom = await _func.GetStoreNameByStoreIdAsync(td.Transfer.StoreFromId),
                            StoreTo = await _func.GetStoreNameByStoreIdAsync(td.Transfer.StoreId),
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
                            CurrentValue = await _func.GetDepreciatedCostAsync(td.AssetId)
                        };

                        dto.Add(outgoingAsset);
                    }
                }

                return Result<List<TransfersOutgoingAssetDTO>>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<List<TransfersOutgoingAssetDTO>>.Failure(ex.Message);
            }
        }
        public async Task<Result<List<TransfersIncomingAssetDTO>>> GetReceivedAssetsAsync(int storeId)
        {
            try
            {
                var transferDetails = _db.TransferDetails
                .Include(td => td.Transfer)
                .Where(td => td.Transfer.StoreId == storeId && td.Transfer.TransferStatusId == 3)
                .ToList();

                var assetIds = transferDetails.Select(td => td.AssetId).ToList();

                var assets = _db.Assets
                    .Include(asset => asset.SubCategory.Category)
                    .Where(asset => assetIds.Contains(asset.Id))
                    .ToList();

                var dto = new List<TransfersIncomingAssetDTO>();

                foreach (var td in transferDetails)
                {
                    var transferAsset = assets.FirstOrDefault(a => a.Id == td.AssetId);

                    if (transferAsset != null)
                    {
                        var incomingAssets = new TransfersIncomingAssetDTO
                        {
                            TransferDate = td.Transfer.TransferDate,
                            StoreFrom = await _func.GetStoreNameByStoreIdAsync(td.Transfer.StoreFromId),
                            StoreTo = await _func.GetStoreNameByStoreIdAsync(td.Transfer.StoreId),
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
                            CurrentValue = await _func.GetDepreciatedCostAsync(td.AssetId)
                        };

                        dto.Add(incomingAssets);
                    }
                }

                return Result<List<TransfersIncomingAssetDTO>>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<List<TransfersIncomingAssetDTO>>.Failure(ex.Message);
            }
        }
        public async Task<List<TransferChartDTO>> GetChartDataAsync(int type)
        {
            List<TransferChartDTO> dtoTransferCharts = new List<TransferChartDTO>();

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
                TransferChartDTO dto = new TransferChartDTO()
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

        //Private Functions
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
        private async Task<string> GetTransferNumberAsync(int currentStoreId)
        {
            string sResult = "";

            var transfers = await _db.Transfers.Where(m => m.EmployeeId == _employeeId).ToListAsync();
            var maxIdTransfer = transfers.OrderByDescending(m => m.Id).FirstOrDefault();

            int maxId = 0;
            if (maxIdTransfer != null)
            {
                maxId = maxIdTransfer.Id;
            }
            ;
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
        private async Task<List<MODAMS.Models.Asset>> GetAssetsAsync([CallerMemberName] string caller = "")
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
        private async Task<int> GetCurrentStoreIdAsync()
        {
            int departmentId = await _func.GetDepartmentIdAsync(_employeeId);
            return await _func.GetStoreIdByDepartmentIdAsync(departmentId);

        }
        private async Task<decimal> GetTotalTransferValueAsync(int storeId)
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
        private async Task<decimal> GetTotalReceivedValueAsync(int storeId)
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
        private async Task<string> GetAssetImageAsync(int assetId)
        {
            var imageUrl = await _db.AssetPictures
                .Where(m => m.AssetId == assetId)
                .OrderByDescending(m => m.Id)
                .Select(m => m.ImageUrl)
                .FirstOrDefaultAsync();

            return imageUrl ?? "/assets/images/placeholders/pictureplaceholder.jpg";
        }
    }
}
