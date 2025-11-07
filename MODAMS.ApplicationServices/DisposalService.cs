using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Hosting;
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
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices
{
    public class DisposalService : IDisposalService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly ILogger<DisposalService> _logger;
        private int _employeeId;
        private int _storeId;
        private bool _isSomali;

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public DisposalService(ApplicationDbContext db, IAMSFunc func, IWebHostEnvironment webHostEnvironment,
            ILogger<DisposalService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _func = func;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            _employeeId = _func.GetEmployeeId();
            _isSomali = CultureInfo.CurrentUICulture.Name == "so";
        }

        public async Task<Result<DisposalsDTO>> GetIndexAsync()
        {
            try
            {
                _storeId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);

                var dto = new DisposalsDTO
                {
                    IsAuthorized = await _func.CanModifyStoreAsync(_storeId, _employeeId),
                    StoreId = _storeId,
                    StoreName = await _func.GetStoreNameByStoreIdAsync(_storeId)
                };

                bool isScopedToStore = IsInRole("User") || IsInRole("StoreOwner");

                var disposalsQuery = _db.Disposals
                    .Include(m => m.DisposalType)
                    .Include(m => m.Asset).ThenInclude(a => a.Store)
                    .Include(m => m.Asset.SubCategory).ThenInclude(sc => sc.Category)
                    .AsQueryable();

                if (isScopedToStore)
                {
                    disposalsQuery = disposalsQuery.Where(m => m.Asset.StoreId == _storeId);
                }

                dto.Disposals = await disposalsQuery.ToListAsync();

                // --- Group for chart: ALWAYS by Type; scope by store only when needed ---
                var groupedByType = await _db.Disposals
                    .Where(d => !isScopedToStore || d.Asset.StoreId == _storeId)
                    .GroupBy(d => d.DisposalType.Type) // English key only
                    .Select(g => new
                    {
                        Type = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var countsDict = groupedByType.ToDictionary(k => k.Type, v => v.Count);
                var disposalTypes = await _db.DisposalTypes.ToListAsync();

                var chartData = disposalTypes.Select(type => new DisposalChart
                {
                    Type = _isSomali ? type.TypeSo : type.Type,
                    StoreId = _storeId,
                    Count = countsDict.TryGetValue(type.Type, out var cnt) ? cnt : 0
                }).ToList();

                dto.ChartData = chartData;

                return Result<DisposalsDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<DisposalsDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<DisposalCreateDTO>> GetCreateDisposalAsync()
        {
            try
            {
                var dto = new DisposalCreateDTO();
                dto = await PopulateDisposalDtoAsync(dto);

                return Result<DisposalCreateDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<DisposalCreateDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<DisposalCreateDTO>> CreateDisposalAsync(DisposalCreateDTO dto)
        {
            if (dto?.Disposal == null)
                return Result<DisposalCreateDTO>.Failure(_isSomali
                    ? "Macluumaadka baabi’inta lama hayo."
                    : "No disposal data provided.");

            var storeId = await _func.GetStoreIdByAssetIdAsync(dto.Disposal.AssetId);
            if (!await _func.CanModifyStoreAsync(storeId, _employeeId))
            {
                return Result<DisposalCreateDTO>.Failure(_isSomali
                    ? "Ma haysatid rukhsad ku habboon inaad wax ka beddesho kaydkan."
                    : "You do not have permission to modify this store.");
            }

            string fileName = string.Empty;
            if (dto.file != null)
            {
                var wwwRoot = _webHostEnvironment.WebRootPath;
                var uploadDir = Path.Combine(wwwRoot, "disposaldocuments");
                Directory.CreateDirectory(uploadDir); // ensure folder exists

                var ext = Path.GetExtension(dto.file.FileName);
                var uniqueName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(uploadDir, uniqueName);

                await using var stream = new FileStream(fullPath, FileMode.Create);
                await dto.file.CopyToAsync(stream);

                fileName = uniqueName;
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var disposal = dto.Disposal;
                disposal.ImageUrl = fileName;
                disposal.EmployeeId = _employeeId;

                _db.Disposals.Add(disposal);

                var asset = await _db.Assets.FindAsync(disposal.AssetId);
                if (asset != null)
                {
                    asset.AssetStatusId = SD.Asset_Disposed;
                }

                var employeeName = await _func.GetEmployeeNameAsync();
                var history = new AssetHistory
                {
                    TimeStamp = DateTime.UtcNow,
                    AssetId = disposal.AssetId,
                    TransactionRecordId = disposal.AssetId,
                    TransactionTypeId = SD.Transaction_Disposal,
                    Description = _isSomali
                        ? $"Hantida waxaa baabi’iyay {employeeName}"
                        : $"Asset disposed by {employeeName}"
                };
                _db.AssetHistory.Add(history);

                await _db.SaveChangesAsync();

                var assetName = await _func.GetAssetNameAsync(disposal.AssetId);
                var storeName = await _func.GetStoreNameByStoreIdAsync(storeId);
                var message = _isSomali
                    ? $"{employeeName} wuxuu baabi’iyay hanti ({assetName}) oo ku taal {storeName}"
                    : $"{employeeName} disposed an asset ({assetName}) in {storeName}";
                await _func.LogNewsFeedAsync(
                    message,
                    "Users",
                    "Disposals",
                    "EditDisposal",
                    disposal.AssetId
                );

                await tx.CommitAsync();

                var resultDto = await PopulateDisposalDtoAsync(dto);
                return Result<DisposalCreateDTO>.Success(resultDto);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _func.LogException(_logger, ex);
                return Result<DisposalCreateDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<DisposalEditDTO>> GetEditDisposalAsync(int id)
        {
            try
            {
                var storeId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);

                var disposal = await _db.Disposals
                    .Where(m => m.Id == id)
                    .Include(m => m.DisposalType)
                    .Include(m => m.Asset).ThenInclude(a => a.Store)
                    .Include(m => m.Asset.SubCategory).ThenInclude(s => s.Category)
                    .FirstOrDefaultAsync();

                if (disposal == null)
                    return Result<DisposalEditDTO>.Failure(_isSomali ? "Baabi’intii lama helin!" : "Disposal not found!");


                var dto = new DisposalEditDTO
                {
                    Disposal = disposal
                };

                dto = await PopulateDisposalDtoAsync(dto);

                var currentDisposedAsset = await _db.Assets
                    .Include(m => m.SubCategory).ThenInclude(m => m.Category)
                    .FirstOrDefaultAsync(m => m.Id == disposal.AssetId);

                if (currentDisposedAsset != null)
                {
                    dto.CurrentDisposedAsset = currentDisposedAsset;
                }

                return Result<DisposalEditDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<DisposalEditDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<DisposalEditDTO>> EditDisposalAsync(DisposalEditDTO dto)
        {
            if (dto?.Disposal == null)
                return Result<DisposalEditDTO>.Failure(_isSomali
                    ? "Macluumaadka baabi’inta lama hayo."
                    : "No disposal data provided.");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var existing = await _db.Disposals
                    .FirstOrDefaultAsync(d => d.Id == dto.Disposal.Id);

                if (existing == null)
                    return Result<DisposalEditDTO>.Failure(_isSomali
                        ? "Diiwaanka lama helin!"
                        : "Record not found!");

                string newFileName = existing.ImageUrl ?? string.Empty;
                if (dto.file != null)
                {
                    var uploaded = await UploadFileAsync(dto.file);
                    if (string.IsNullOrEmpty(uploaded))
                        return Result<DisposalEditDTO>.Failure(_isSomali
                            ? "Soo gelinta faylka way guuldareysatay"
                            : "File upload failed");

                    // Delete old file (if any)
                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                        await DeleteFileAsync(existing.ImageUrl, "disposaldocuments");

                    newFileName = uploaded;
                }

                var employeeName = await _func.GetEmployeeNameAsync();
                int previousAssetId = existing.AssetId;
                int newAssetId = dto.Disposal.AssetId;

                existing.ImageUrl = newFileName;
                existing.EmployeeId = _employeeId;
                existing.AssetId = newAssetId;
                existing.Notes = dto.Disposal.Notes;

                if (newAssetId != previousAssetId)
                {
                    // previous asset: mark available + history
                    await UpdateAssetStatusAsync(previousAssetId, SD.Asset_Available);
                    await AddAssetHistoryAsync(previousAssetId,
                        _isSomali
                            ? $"Hantida dib ayaa loo celiyay {employeeName}"
                            : $"Asset un-disposed by {employeeName}");

                    // new asset: mark disposed + history
                    await UpdateAssetStatusAsync(newAssetId, SD.Asset_Disposed);
                    await AddAssetHistoryAsync(newAssetId,
                        _isSomali
                            ? $"Hantida waxaa baabi’iyay {employeeName}"
                            : $"Asset disposed by {employeeName}");
                }
                else
                {
                    // if AssetId unchanged, just update its status to disposed
                    await UpdateAssetStatusAsync(newAssetId, SD.Asset_Disposed);
                }

                await LogNewsFeedAsync(
                    disposalId: existing.Id,
                    assetId: newAssetId
                );

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                var resultDto = await PopulateDisposalDtoAsync(dto);
                return Result<DisposalEditDTO>.Success(resultDto);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _func.LogException(_logger, ex);
                return Result<DisposalEditDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<DisposalPreviewDTO>> GetDisposalPreviewAsync(int id)
        {
            try
            {
                var dto = await _db.Disposals
                    .AsNoTracking()
                    .Where(d => d.Id == id)
                    .Select(d => new DisposalPreviewDTO
                    {
                        Id = d.Id,
                        DepartmentName = _isSomali ? d.Asset.Store.Department.NameSo : d.Asset.Store.Department.Name,
                        DisposalType = _isSomali ? d.DisposalType.TypeSo : d.DisposalType.Type,
                        SubCategoryName = _isSomali ? d.Asset.SubCategory.SubCategoryNameSo : d.Asset.SubCategory.SubCategoryName,
                        AssetName = d.Asset.Name,
                        Identification =
                            d.Asset.SubCategory.Category.CategoryName == "Vehicles"
                                ? (_isSomali ? "Taarikada: " : "Plate No: ") + d.Asset.Plate
                                : (_isSomali ? "Sereelka: " : "SN: ") + d.Asset.SerialNo,
                        DisposalDate = d.DisposalDate,
                        ImageUrl = string.IsNullOrEmpty(d.ImageUrl)
                            ? string.Empty
                            : $"/disposaldocuments/{d.ImageUrl}",
                        DisposalNotes = d.Notes ?? string.Empty
                    })
                    .FirstOrDefaultAsync();

                if (dto == null)
                    return Result<DisposalPreviewDTO>.Failure(
                        _isSomali ? "Baabi’in lama helin!" : "Disposal not found!");

                return Result<DisposalPreviewDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<DisposalPreviewDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<bool>> DeleteDisposalAsync(int id)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var disposalInDb = await _db.Disposals.FirstOrDefaultAsync(m => m.Id == id);
                    if (disposalInDb == null)
                    {
                        return Result<bool>.Failure(_isSomali ? "Baabi’in lama helin" : "Disposal not found.");
                    }

                    int assetId = disposalInDb.AssetId;
                    var assetInDb = await _db.Assets
                        .Include(m => m.Store)
                        .FirstOrDefaultAsync(m => m.Id == assetId);
                    if (assetInDb != null)
                    {
                        assetInDb.AssetStatusId = SD.Asset_Available;
                    }
                    _db.Disposals.Remove(disposalInDb);
                    await _db.SaveChangesAsync();

                    // Log NewsFeed
                    string employeeName = await _func.GetEmployeeNameAsync();
                    string assetName = assetInDb?.Name ?? "-";
                    var storeName = _isSomali ? assetInDb?.Store?.NameSo : assetInDb?.Store?.Name ?? "-";

                    string message = _isSomali
                    ? $"{employeeName} wuxuu joojiyay baabi’inta hanti ({assetName}) oo ku taal {storeName}"
                    : $"{employeeName} un-disposed an asset ({assetName}) in {storeName}";

                    await _func.LogNewsFeedAsync(message, "Users", "Disposals", "Index", assetId);
                    await AddAssetHistoryAsync(assetId, _isSomali
                    ? $"Hantida waxaa dib loo furay uu sameeyay {employeeName}"
                    : $"Asset Un-Disposed by {employeeName}");
                    await transaction.CommitAsync();

                    var result = await DeletePictureAsync(id);

                    return Result<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _func.LogException(_logger, ex);
                    return Result<bool>.Failure(ex.Message);
                }
            }
        }
        public async Task<T> PopulateDisposalDtoAsync<T>(T dto) where T : class
        {
            // Check if the DTO has the necessary properties using reflection
            var isAuthorizedField = dto.GetType().GetProperty("IsAuthorized");
            var storeNameField = dto.GetType().GetProperty("StoreName");
            var storeOwnerField = dto.GetType().GetProperty("StoreOwner");
            var assetsField = dto.GetType().GetProperty("Assets");
            var disposalTypeListField = dto.GetType().GetProperty("DisposalTypeList");

            _storeId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);

            // Populate Assets
            if (assetsField != null)
            {
                var assetList = await _db.Assets
                .Include(m => m.SubCategory).ThenInclude(m => m.Category)
                .Where(m => m.AssetStatusId == SD.Asset_Available && m.StoreId == _storeId)
                .Select(a => new DisposalAssetDto
                {
                    Id = a.Id,
                    CategoryName = _isSomali ? a.SubCategory.Category.CategoryNameSo : a.SubCategory.Category.CategoryName,
                    SubCategoryName = _isSomali ? a.SubCategory.SubCategoryNameSo : a.SubCategory.SubCategoryName,
                    AssetName = a.Name,
                    Make = a.Make,
                    Model = a.Model,
                    Barcode = a.Barcode,
                    Identification =
                    a.SubCategory.Category.CategoryName == "Vehicles"
                    ? _func.BuildVehicleIdentification(a.Plate, a.Chasis, a.Engine)
                    : ((_isSomali ? "Sereelka: " : "SN: ") + (a.SerialNo ?? ""))
                })
                .ToListAsync();
                assetsField.SetValue(dto, assetList);
            }

            // Populate IsAuthorized (only if the property exists)
            if (isAuthorizedField != null)
            {
                var isAuthorized = await _func.CanModifyStoreAsync(_storeId, _employeeId);
                isAuthorizedField.SetValue(dto, isAuthorized);
            }

            // Populate StoreName
            if (storeNameField != null)
            {
                var storeName = await _func.GetStoreNameByStoreIdAsync(_storeId);
                storeNameField.SetValue(dto, storeName);
            }

            // Populate StoreOwner
            if (storeOwnerField != null)
            {
                var storeOwner = await _func.GetEmployeeNameByIdAsync(await _func.GetStoreOwnerIdAsync(_storeId));
                storeOwnerField.SetValue(dto, storeOwner);
            }

            // Populate DisposalTypeList
            if (disposalTypeListField != null)
            {
                var disposalTypeList = _db.DisposalTypes.ToList().Select(m => new SelectListItem
                {
                    Text = _isSomali ? m.TypeSo : m.Type,
                    Value = m.Id.ToString()
                });
                disposalTypeListField.SetValue(dto, disposalTypeList);
            }

            return dto;
        }

        //Private Functions
        private bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        private async Task<Result<string>> DeletePictureAsync(int disposalId)
        {
            if (disposalId == 0)
            {
                return Result<string>.Failure(_isSomali ? "Baabi’in lama helin!" : "Disposal not found!");
            }

            var disposalInDb = await _db.Disposals.FirstOrDefaultAsync(m => m.Id == disposalId);

            if (disposalInDb == null)
            {
                return Result<string>.Failure(_isSomali ? "Baabi’in lama helin!" : "Disposal not found!");
            }

            // Remove file from disk
            var sFileName = disposalInDb?.ImageUrl?.Substring(19) ?? ""; // Removes "/disposaldocuments/"
            bool fileDeleted = await DeleteFileAsync(sFileName, "disposaldocuments");
            if (!fileDeleted)
            {
                return Result<string>.Failure(_isSomali ? "Khalad ayaa ka dhacay tirtirka sawirka kaydka!" : "Error deleting picture from storage!");
            }

            return Result<string>.Success(_isSomali ? "Sawirku si guul leh ayaa loo tirtiray!" : "Picture deleted successfully!");
        }
        private async Task<bool> DeleteFileAsync(string fileName, string folderName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(folderName))
                return false;

            var wwwRoot = _webHostEnvironment.WebRootPath;
            var filePath = Path.Combine(wwwRoot, folderName, fileName);

            if (!System.IO.File.Exists(filePath))
                return false;

            try
            {
                await Task.Run(() => System.IO.File.Delete(filePath));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file at {FilePath}", filePath);
                return false;
            }
        }
        private async Task<string> UploadFileAsync(IFormFile file)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string targetDir = "disposaldocuments";
            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(wwwRootPath, targetDir, uniqueFileName);

            try
            {
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                return uniqueFileName;
            }
            catch
            {
                return string.Empty;
            }
        }
        private async Task UpdateAssetStatusAsync(int assetId, int statusId)
        {
            var asset = await _db.Assets.FirstOrDefaultAsync(m => m.Id == assetId);
            if (asset != null)
            {
                asset.AssetStatusId = statusId;
            }
        }
        private async Task AddAssetHistoryAsync(int assetId, string description)
        {
            var assetHistory = new AssetHistory
            {
                TimeStamp = DateTime.Now,
                AssetId = assetId,
                TransactionRecordId = assetId,
                TransactionTypeId = SD.Transaction_Disposal,
                Description = description
            };
            await _db.AssetHistory.AddAsync(assetHistory);
        }
        private async Task LogNewsFeedAsync(int disposalId, int assetId)
        {
            string employeeName = await _func.GetEmployeeNameAsync();
            string assetName = await _func.GetAssetNameAsync(assetId);
            string storeName = await _func.GetStoreNameByStoreIdAsync(await _func.GetStoreIdByAssetIdAsync(assetId));
            string message = _isSomali
            ? $"{employeeName} wuxuu wax ka beddelay diiwaanka baabi’inta ee hanti ({assetName}) oo ku taal {storeName}"
            : $"{employeeName} modified disposal record for an asset ({assetName}) in {storeName}";

            await _func.LogNewsFeedAsync(message, "Users", "Disposals", "EditDisposal", disposalId);
        }
    }
}
