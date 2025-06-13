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
using System.Globalization;
using System.Runtime.CompilerServices;


namespace MODAMS.ApplicationServices
{
    public class TransferService : ITransferService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly ILogger<TransferService> _logger;

        private int _employeeId;
        private int _storeId;
        private readonly bool _isSomali;

        public TransferService(ApplicationDbContext db, IAMSFunc func, IHttpContextAccessor httpContextAccessor, ILogger<TransferService> logger)
        {
            _db = db;
            _func = func;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            _employeeId = _func.GetEmployeeId();
            _isSomali = CultureInfo.CurrentUICulture.Name == "so";
        }
        public async Task<Result<TransferDTO>> GetIndexAsync(int id = 0, int transferStatusId = 0)
        {
            try
            {
                var dto = new TransferDTO();
                var allStores = await _db.vwStores.ToListAsync();
                var accessibleStores = await GetAccessibleStoresWithTransfersOnlyAsync(allStores);

                _storeId = id > 0
                    ? id
                    : accessibleStores.FirstOrDefault()?.Id ?? 0;

                dto.StoreList = accessibleStores.Select(s => new SelectListItem
                {
                    Text = _isSomali ? s.NameSo : s.Name,
                    Value = s.Id.ToString(),
                    Selected = s.Id == _storeId
                }).ToList();

                dto.IsAuthorized = await _func.CanModifyStoreAsync(_storeId, _employeeId);
                dto.StoreId = _storeId;
                dto.TransferStatus = transferStatusId;

                var outgoingQuery = _db.vwTransfers.Where(m => m.StoreFromId == _storeId);
                if (transferStatusId > 0)
                    outgoingQuery = outgoingQuery.Where(m => m.TransferStatusId == transferStatusId);

                dto.OutgoingTransfers = await outgoingQuery.OrderBy(m => m.TransferStatusId).ToListAsync();

                dto.IncomingTransfers = await _db.vwTransfers
                    .Where(m => m.StoreId == _storeId && m.TransferStatusId != SD.Transfer_Pending)
                    .OrderBy(m => m.TransferStatusId)
                    .ToListAsync();

                dto.OutgoingChartData = await GetChartDataAsync(1);
                dto.IncomingChartData = await GetChartDataAsync(2);
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
                var storeFromId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);
                var storeFrom = await _func.GetDepartmentNameAsync(_employeeId);

                var assets = await _db.Assets
                    .AsNoTracking()
                    .Include(a => a.SubCategory.Category)
                    .Where(a => a.AssetStatusId == SD.Asset_Available
                             && a.StoreId == storeFromId)
                    .ToListAsync();

                var pendingIds = await _db.TransferDetails
                    .AsNoTracking()
                    .Where(td => td.Transfer.TransferStatusId == SD.Transfer_Pending
                              || td.Transfer.TransferStatusId == SD.Transfer_SubmittedForAcknowledgement)
                    .Select(td => td.AssetId)
                    .Distinct()
                    .ToListAsync();

                var imageMap = await _db.AssetPictures
                    .AsNoTracking()
                    .Where(p => assets.Select(a => a.Id).Contains(p.AssetId))
                    .GroupBy(p => p.AssetId)
                    .Select(g => new { g.Key, Url = g.First().ImageUrl })
                    .ToDictionaryAsync(x => x.Key, x => x.Url);

                var transferAssets = assets
                    .Where(a => !pendingIds.Contains(a.Id))
                    .Select(a => new TransferAssetDTO
                    {
                        AssetId = a.Id,
                        AssetName = a.Name,
                        Category = _isSomali
                                         ? a.SubCategory.Category.CategoryNameSo
                                         : a.SubCategory.Category.CategoryName,
                        SubCategory = _isSomali
                                         ? a.SubCategory.SubCategoryNameSo
                                         : a.SubCategory.SubCategoryName,
                        Make = a.Make,
                        Model = a.Model,
                        Barcode = a.Barcode ?? string.Empty,
                        SerialNumber = a.SerialNo,
                        ImageUrl = imageMap.GetValueOrDefault(a.Id, string.Empty),
                        IsSelected = false
                    })
                    .ToList();

                var dto = new TransferCreateDTO
                {
                    StoreFrom = storeFrom,
                    Assets = transferAssets,
                };

                dto.StoreList = await _db.Stores
                    .AsNoTracking()
                    .Where(s => s.Id != storeFromId)
                    .Select(s => new SelectListItem
                    {
                        Text = _isSomali ? s.NameSo : s.Name,
                        Value = s.Id.ToString(),
                    })
                    .ToListAsync();

                dto.Transfer.TransferNumber = await GetTransferNumberAsync(storeFromId);

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
                var storeFromId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);
                var prevStoreId = await GetCurrentStoreIdAsync();

                using var tx = await _db.Database.BeginTransactionAsync();
                
                var transfer = new Transfer
                {
                    TransferDate = transferDTO.Transfer.TransferDate,
                    EmployeeId = _employeeId,
                    StoreFromId = storeFromId,
                    StoreId = transferDTO.Transfer.StoreId,
                    TransferNumber = transferDTO.Transfer.TransferNumber,
                    TransferStatusId = SD.Transfer_Pending,
                    Notes = transferDTO.Transfer.Notes ?? "-",
                    SubmissionForAcknowledgementDate = DateTime.UtcNow
                };

                await _db.Transfers.AddAsync(transfer);
                await _db.SaveChangesAsync();

                var assetIds = (selectedAssets ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(idText => int.TryParse(idText, out var id) ? id : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (assetIds.Any())
                {
                    var assetNames = await _db.Assets
                        .Where(a => assetIds.Contains(a.Id))
                        .Select(a => a.Name)
                        .ToListAsync();
                    
                    var details = assetIds.Select(id => new TransferDetail
                    {
                        AssetId = id,
                        PrevStoreId = prevStoreId,
                        TransferId = transfer.Id
                    });
                    await _db.TransferDetails.AddRangeAsync(details);
                    await _db.SaveChangesAsync();
                }

                await tx.CommitAsync();

                transferDTO.Transfer.Id = transfer.Id;
                return Result<TransferCreateDTO>.Success(transferDTO);
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
                var transfer = await _db.Transfers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == transferId);

                if (transfer == null)
                    return Result<TransferEditDTO>.Failure(
                        _isSomali
                          ? "Wareejin lama helin!"
                          : "Transfer not found!");

                var currentStoreId = await GetCurrentStoreIdAsync();

                var assets = await _db.Assets
                    .AsNoTracking()
                    .Include(a => a.SubCategory)
                        .ThenInclude(sc => sc.Category)
                    .Where(a => a.AssetStatusId == SD.Asset_Available
                             && a.StoreId == currentStoreId)
                    .ToListAsync();

                var selectedAssetIds = await _db.TransferDetails
                    .AsNoTracking()
                    .Where(td => td.TransferId == transferId)
                    .Select(td => td.AssetId)
                    .ToListAsync();

                var imageMap = await _db.AssetPictures
                    .AsNoTracking()
                    .Where(p => assets.Select(a => a.Id).Contains(p.AssetId))
                    .GroupBy(p => p.AssetId)
                    .Select(g => new { AssetId = g.Key, Url = g.First().ImageUrl })
                    .ToDictionaryAsync(x => x.AssetId, x => x.Url);

                var transferAssets = assets
                    .Select(a => new TransferAssetDTO
                    {
                        AssetId = a.Id,
                        AssetName = a.Name,
                        Category = _isSomali
                                          ? a.SubCategory.Category.CategoryNameSo
                                          : a.SubCategory.Category.CategoryName,
                        SubCategory = _isSomali
                                          ? a.SubCategory.SubCategoryNameSo
                                          : a.SubCategory.SubCategoryName,
                        Make = a.Make,
                        Model = a.Model,
                        Barcode = a.Barcode ?? string.Empty,
                        SerialNumber = a.SerialNo,
                        IsSelected = selectedAssetIds.Contains(a.Id),
                        ImageUrl = imageMap.GetValueOrDefault(a.Id, string.Empty)
                    })
                    .OrderByDescending(x => x.IsSelected)
                    .ToList();

                var storeList = await _db.Stores
                    .AsNoTracking()
                    .Where(s => s.Id != currentStoreId)
                    .Select(s => new SelectListItem
                    {
                        Text = _isSomali ? s.NameSo : s.Name,
                        Value = s.Id.ToString()
                    })
                    .ToListAsync();

                var dto = new TransferEditDTO
                {
                    StoreFrom = await _func.GetDepartmentNameAsync(_employeeId),
                    Transfer = transfer,
                    Assets = transferAssets,
                    StoreList = storeList
                };

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
                var storeFromId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);
                var prevStoreId = await GetCurrentStoreIdAsync();

                var transfer = await _db.Transfers
                    .FirstOrDefaultAsync(t => t.Id == transferDTO.Transfer.Id);

                if (transfer == null)
                    return Result<TransferEditDTO>.Failure(
                        _isSomali ? "Wareejin lama helin!" : "Transfer not found!",
                        transferDTO);

                var assetIds = (selectedAssets ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(text => int.TryParse(text, out var id) ? (int?)id : null)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToList();

                using var tx = await _db.Database.BeginTransactionAsync();

                transfer.TransferDate = transferDTO.Transfer.TransferDate;
                transfer.EmployeeId = _employeeId;
                transfer.StoreFromId = storeFromId;
                transfer.StoreId = transferDTO.Transfer.StoreId;
                transfer.TransferNumber = transferDTO.Transfer.TransferNumber;
                transfer.TransferStatusId = SD.Transfer_Pending;
                transfer.Notes = transferDTO.Transfer.Notes ?? "-";

                var existingDetails = await _db.TransferDetails
                    .Where(td => td.TransferId == transfer.Id)
                    .ToListAsync();
                _db.TransferDetails.RemoveRange(existingDetails);

                if (assetIds.Count > 0)
                {
                    var newDetails = assetIds.Select(aid => new TransferDetail
                    {
                        AssetId = aid,
                        PrevStoreId = prevStoreId,
                        TransferId = transfer.Id
                    });
                    await _db.TransferDetails.AddRangeAsync(newDetails);
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return Result<TransferEditDTO>.Success(transferDTO);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<TransferEditDTO>.Failure(ex.Message, transferDTO);
            }
        }
        public async Task<Result<TransferPreviewDTO>> GetPreviewTransferAsync(int transferId)
        {
            try
            {
                var transfer = await _db.vwTransfers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Id == transferId);

                if (transfer == null)
                    return Result<TransferPreviewDTO>.Failure(
                        _isSomali ? "Wareejin lama helin!" : "Transfer not found!");

                var transferAssets = await _db.TransferDetails
                    .AsNoTracking()
                    .Where(td => td.TransferId == transferId)
                    .Select(td => new TransferAssetDTO
                    {
                        AssetId = td.Asset.Id,
                        AssetName = td.Asset.Name,
                        Category = _isSomali
                                         ? td.Asset.SubCategory.Category.CategoryNameSo
                                         : td.Asset.SubCategory.Category.CategoryName,
                        SubCategory = _isSomali
                                         ? td.Asset.SubCategory.SubCategoryNameSo
                                         : td.Asset.SubCategory.SubCategoryName,
                        Make = td.Asset.Make,
                        Model = td.Asset.Model,
                        Barcode = td.Asset.Barcode ?? "-",
                        SerialNumber = td.Asset.SubCategory.Category.CategoryName == "Vehicles"
                            ? (_isSomali ? "Taariko: " : "Plate No: ") + td.Asset.Plate
                            : (_isSomali ? "L.T: " : "S.N: ") + td.Asset.SerialNo,
                        Condition = _isSomali
                                         ? td.Asset.Condition.ConditionNameSo
                                         : td.Asset.Condition.ConditionName,
                        IsSelected = true
                    })
                    .OrderByDescending(a => a.Category == (_isSomali ? "Gaadiid" : "Vehicles")) 
                    .ToListAsync();

                var senderId = await _func.GetStoreOwnerIdAsync(transfer.StoreFromId);
                var receiverId = await _func.GetStoreOwnerIdAsync(transfer.StoreId);
                bool isSender = senderId == _employeeId;
                bool isReceiver = receiverId == _employeeId;

                string transferBy = transfer.SenderBarcode ?? "";
                string receivedBy = transfer.ReceiverBarcode ?? "";

                var fromSignature = !string.IsNullOrWhiteSpace(transferBy)
                    ? Barcode.GenerateBarCode(transferBy)
                    : null;

                var toSignature = !string.IsNullOrWhiteSpace(receivedBy)
                    ? Barcode.GenerateBarCode(receivedBy)
                    : null;

                var dto = new TransferPreviewDTO
                {
                    vwTransfer = transfer,
                    transferAssets = transferAssets,
                    IsSender = isSender,
                    IsReceiver = isReceiver,
                    TransferBy = transferBy,
                    ReceivedBy = receivedBy,
                    FromSignature = fromSignature ?? "",
                    ToSignature = toSignature ?? ""
                };

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
                    return Result<string>.Failure(_isSomali ? "Diiwaanka wareejinta lama helin!" : "Transfer record not found!");
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
                var transfer = await _db.Transfers
                    .FirstOrDefaultAsync(t => t.Id == transferId);

                if (transfer == null)
                    return Result.Failure(
                        _isSomali
                          ? "Diiwaanka wareejinta lama helin!"
                          : "Transfer not found!");

                transfer.TransferStatusId = SD.Transfer_SubmittedForAcknowledgement;
                transfer.SenderBarcode = await _func.GetEmployeeNameAsync(_employeeId);

                await _db.SaveChangesAsync();

                var senderName = await _func.GetEmployeeNameByIdAsync(_employeeId);
                var receiverId = await _func.GetStoreOwnerIdAsync(transfer.StoreId);
                var departmentId = await _func.GetDepartmentIdByEmployeeIdAsync(receiverId);

                string subject = _isSomali
                    ? "Wareejintu waxay sugaysaa oggolaansho"
                    : "Transfer awaiting acknowledgement";

                string messageTemplate = _isSomali
                    ? "Wareejin cusub waxaa soo gudbiyay {0} si aad u oggolaato. Fadlan guji xiriirka hoose oo raac tilmaamaha."
                    : "A new transfer has been submitted by {0} for your acknowledgement. Please click the following link and follow the instructions.";

                var notification = new Notification
                {
                    EmployeeFrom = _employeeId,
                    EmployeeTo = receiverId,
                    Subject = subject,
                    Message = string.Format(messageTemplate, senderName),
                    DateTime = DateTime.UtcNow,
                    IsViewed = false,
                    TargetRecordId = transfer.Id,
                    NotificationSectionId = SD.NS_Transfer
                };

                await _func.NotifyDepartmentAsync(departmentId, notification);

                var actorName = await _func.GetEmployeeNameAsync();
                string newsMessage = _isSomali
                    ? $"{actorName} wuxuu soo gudbiyay wareejinta ({transfer.TransferNumber}) si loo oggolaado"
                    : $"{actorName} submitted the transfer ({transfer.TransferNumber}) for acknowledgement";

                await _func.LogNewsFeedAsync(
                    newsMessage,
                    area: "Users",
                    controller: "Transfers",
                    action: "PreviewTransfer",
                    sourceRecordId: transfer.Id);

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
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var transfer = await _db.Transfers
                    .FirstOrDefaultAsync(t => t.Id == transferId);

                if (transfer == null)
                    return Result.Failure(
                        _isSomali
                          ? "Diiwaanka wareejinta lama helin!"
                          : "Transfer not found!");

                var assetIds = await _db.TransferDetails
                    .Where(td => td.TransferId == transferId)
                    .Select(td => td.AssetId)
                    .ToListAsync();

                if (assetIds.Any())
                {
                    var assets = await _db.Assets
                        .Where(a => assetIds.Contains(a.Id))
                        .ToListAsync();

                    var fromStore = await _func.GetStoreNameByStoreIdAsync(transfer.StoreFromId);
                    var toStore = await _func.GetStoreNameByStoreIdAsync(transfer.StoreId);

                    var histories = assets.Select(a => new AssetHistory
                    {
                        AssetId = a.Id,
                        Description = _isSomali
                            ? $"Hanti ayaa laga wareejiyay {fromStore} loona wareejiyay {toStore}"
                            : $"Asset Transferred from {fromStore} to {toStore}",
                        TimeStamp = DateTime.UtcNow,
                        TransactionRecordId = transferId,
                        TransactionTypeId = SD.Transaction_Transfer
                    }).ToList();

                    _db.AssetHistory.AddRange(histories);

                    assets.ForEach(a => a.StoreId = transfer.StoreId);
                }

                transfer.TransferStatusId = SD.Transfer_Completed;
                transfer.ReceiverBarcode = await _func.GetEmployeeNameAsync(_employeeId);
                transfer.AcknowledgementDate = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                var ownerId = await _func.GetStoreOwnerIdAsync(transfer.StoreFromId);
                var actor = await _func.GetEmployeeNameByIdAsync(_employeeId);

                string notifSubject = _isSomali
                    ? "Wareejinta waa la oggolaaday"
                    : "Transfer acknowledged";

                string notifMessage = _isSomali
                    ? $"{actor} ayaa oggolaaday wareejinta ({transfer.TransferNumber})."
                    : $"Transfer Number: <b>{transfer.TransferNumber}</b> has been acknowledged by {actor}.";

                var notification = new Notification
                {
                    EmployeeFrom = _employeeId,
                    EmployeeTo = ownerId,
                    Subject = notifSubject,
                    Message = notifMessage,
                    DateTime = DateTime.UtcNow,
                    IsViewed = false,
                    TargetRecordId = transferId,
                    NotificationSectionId = SD.NS_Transfer
                };

                var deptId = await _func.GetDepartmentIdByEmployeeIdAsync(ownerId);
                await _func.NotifyDepartmentAsync(deptId, notification);

                string newsMsg = _isSomali
                    ? $"{actor} ayaa oggolaaday wareejinta ({transfer.TransferNumber})"
                    : $"{actor} acknowledged the transfer ({transfer.TransferNumber})";

                await _func.LogNewsFeedAsync(
                    newsMsg,
                    area: "Users",
                    controller: "Transfers",
                    action: "PreviewTransfer",
                    sourceRecordId: transferId);

                await tx.CommitAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _func.LogException(_logger, ex);

                var err = _isSomali
                    ? "Khalad ayaa dhacay inta lagu jiray oggolaanshaha wareejinta"
                    : "Error acknowledging transfer";

                return Result.Failure($"{err}: {ex.Message}");
            }
        }
        public async Task<Result> RejectTransferAsync(int transferId, string txtReason = "")
        {
            try
            {
                var transfer = await _db.Transfers
                    .FirstOrDefaultAsync(t => t.Id == transferId);
                if (transfer == null)
                    return Result.Failure(
                        _isSomali
                          ? "Diiwaanka wareejinta lama helin!"
                          : "Transfer record not found!");

                transfer.TransferStatusId = SD.Transfer_Rejected;
                transfer.AcknowledgementDate = DateTime.UtcNow;
                transfer.ReceiverBarcode = await _func.GetEmployeeNameAsync(_employeeId);

                await _db.SaveChangesAsync();

                var ownerId = await _func.GetStoreOwnerIdAsync(transfer.StoreFromId);
                var actorName = await _func.GetEmployeeNameByIdAsync(_employeeId);
                var transferNum = transfer.TransferNumber;

                string subject = _isSomali
                    ? "Wareejinta waa la diiday"
                    : "Transfer rejected";

                string notifTemplate = _isSomali
                    ? "Lambarka Wareejinta: <b>{0}</b> waxaa diiday {1}, fadlan guji xiriirka hoose si aad u aragto faahfaahinta."
                    : "Transfer Number: <b>{0}</b> has been rejected by {1}, please click the following link for details.";

                var notification = new Notification
                {
                    EmployeeFrom = _employeeId,
                    EmployeeTo = ownerId,
                    Subject = subject,
                    Message = string.Format(notifTemplate, transferNum, actorName),
                    DateTime = DateTime.UtcNow,
                    IsViewed = false,
                    TargetRecordId = transfer.Id,
                    NotificationSectionId = SD.NS_Transfer
                };

                var departmentId = await _func.GetDepartmentIdByEmployeeIdAsync(ownerId);
                await _func.NotifyDepartmentAsync(departmentId, notification);

                string newsTemplate = _isSomali
                    ? "{1} ayaa diiday wareejinta ({0})"
                    : "{1} rejected the transfer ({0})";

                await _func.LogNewsFeedAsync(
                    string.Format(newsTemplate, transferNum, actorName),
                    area: "Users",
                    controller: "Transfers",
                    action: "PreviewTransfer",
                    sourceRecordId: transfer.Id);

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
                    .Include(asset => asset.Condition)
                    .Where(asset => assetIds.Contains(asset.Id))
                    .ToListAsync();

                var dto = new List<TransfersOutgoingAssetDTO>();

                foreach (var td in transferDetails)
                {
                    var transferAsset = assets.FirstOrDefault(a => a.Id == td.AssetId);

                    if (transferAsset != null)
                    {
                        var subCategory = transferAsset.SubCategory;
                        var category = subCategory?.Category;
                        var condition = transferAsset.Condition;

                        var outgoingAsset = new TransfersOutgoingAssetDTO
                        {
                            TransferDate = td.Transfer.TransferDate,
                            StoreFrom = await _func.GetStoreNameByStoreIdAsync(td.Transfer.StoreFromId),
                            StoreTo = await _func.GetStoreNameByStoreIdAsync(td.Transfer.StoreId),
                            AssetId = td.AssetId,
                            Category = _isSomali
                                ? category?.CategoryNameSo ?? ""
                                : category?.CategoryName ?? "",
                            SubCategory = _isSomali
                                ? subCategory?.SubCategoryNameSo ?? ""
                                : subCategory?.SubCategoryName ?? "",
                            Make = transferAsset.Make,
                            Model = transferAsset.Model,
                            AssetName = transferAsset.Name,
                            Barcode = transferAsset.Barcode ?? "",
                            Condition = _isSomali
                                ? condition?.ConditionNameSo ?? ""
                                : condition?.ConditionName ?? "",
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
                        var subCategory = transferAsset.SubCategory;
                        var category = subCategory?.Category;
                        var condition = transferAsset.Condition;

                        var incomingAssets = new TransfersIncomingAssetDTO
                        {
                            TransferDate = td.Transfer.TransferDate,
                            StoreFrom = await _func.GetStoreNameByStoreIdAsync(td.Transfer.StoreFromId),
                            StoreTo = await _func.GetStoreNameByStoreIdAsync(td.Transfer.StoreId),
                            AssetId = td.AssetId,
                            Category = _isSomali
                                ? category?.CategoryNameSo ?? ""
                                : category?.CategoryName ?? "",
                            SubCategory = _isSomali
                                ? subCategory?.SubCategoryNameSo ?? ""
                                : subCategory?.SubCategoryName ?? "",
                            Make = transferAsset.Make,
                            Model = transferAsset.Model,
                            AssetName = transferAsset.Name,
                            Barcode = transferAsset.Barcode ?? "",
                            Condition = _isSomali
                                ? condition?.ConditionNameSo ?? ""
                                : condition?.ConditionName ?? "",
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
                .Where(combined => combined.transfer.TransferStatusId == SD.Transfer_Completed)
                .GroupBy(combined => new
                {
                    combined.subCategory.Id,
                    combined.subCategory.SubCategoryName,
                    combined.subCategory.SubCategoryNameSo,
                    combined.transfer.StoreFromId,
                    combined.transfer.StoreId
                })
                .Select(grouped => new
                {
                    Id = grouped.Key.Id,
                    SubCategoryName = _isSomali ? grouped.Key.SubCategoryNameSo : grouped.Key.SubCategoryName,
                    TotalAssets = grouped.Count(),
                    StoreFromId = grouped.Key.StoreFromId,
                    StoreToId = grouped.Key.StoreId
                })
                .ToListAsync();

            foreach (var item in result)
            {
                dtoTransferCharts.Add(new TransferChartDTO
                {
                    Id = item.Id,
                    SubCategoryName = item.SubCategoryName,
                    TotalAssets = item.TotalAssets,
                    StoreFromId = item.StoreFromId,
                    StoreToId = item.StoreToId
                });
            }

            if (type == 1)
                dtoTransferCharts = dtoTransferCharts.Where(m => m.StoreFromId == _storeId).ToList();
            else
                dtoTransferCharts = dtoTransferCharts.Where(m => m.StoreToId == _storeId).ToList();

            return dtoTransferCharts;
        }


        //Private Functions
        private bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
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
            var maxId = await _db.Transfers
                .Where(m => m.EmployeeId == _employeeId)
                .MaxAsync(m => (int?)m.Id) ?? 0;

            maxId++; // Increment for the next transfer

            string paddedNumber = maxId.ToString("D5"); // Pads with leading zeros to make 5 digits

            return $"{currentStoreId}-{paddedNumber}";
        }
        private async Task<List<Asset>> GetAssetsAsync([CallerMemberName] string caller = "")
        {
            var storeId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);

            var query = _db.Assets
                .AsNoTracking()
                .Include(a => a.Store.Department)
                .Include(a => a.SubCategory.Category)
                .Where(a => a.AssetStatusId == SD.Asset_Available
                         && a.StoreId == storeId);

            if (caller == nameof(GetCreateTransferAsync))
            {
                var pendingStatuses = new[]
                {
                    SD.Transfer_Pending,
                    SD.Transfer_SubmittedForAcknowledgement
                };

                query = query.Where(a => !_db.TransferDetails
                        .Where(td => pendingStatuses.Contains(td.Transfer.TransferStatusId))
                        .Select(td => td.AssetId)
                        .Contains(a.Id));
            }

            return await query.ToListAsync();
        }
        private async Task<int> GetCurrentStoreIdAsync()
        {
            int departmentId = await _func.GetDepartmentIdByEmployeeIdAsync(_employeeId);
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
        private async Task<List<vwStore>> GetAccessibleStoresWithTransfersOnlyAsync(List<vwStore> allStores)
        {
            List<vwStore> stores = new();

            if (IsInRole("User"))
            {
                int storeId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);
                stores = allStores.Where(s => s.Id == storeId).ToList();
            }
            else if (IsInRole("StoreOwner"))
            {
                var ownedStores = allStores.Where(s => s.EmployeeId == _employeeId).ToList();
                if (ownedStores.Any())
                {
                    var mainDeptId = ownedStores.First().DepartmentId;
                    var storeFinder = new StoreFinder(mainDeptId, allStores);
                    stores = storeFinder.GetStores();
                }
            }
            else
            {
                stores = allStores;
            }

            // Filter to stores that have at least one transfer record
            var storeIdsWithTransfers = await _db.vwTransfers
                .Select(t => t.StoreFromId)
                .Distinct()
                .ToListAsync();

            return stores.Where(s => storeIdsWithTransfers.Contains(s.Id)).ToList();
        }

    }
}
