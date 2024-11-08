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
using System.Linq;
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
        }


        public async Task<Result<DisposalsDTO>> GetIndexAsync()
        {
            try
            {
                _employeeId = IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
                _storeId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);

                var dto = new DisposalsDTO();

                if (_employeeId == await _func.GetStoreOwnerIdAsync(_employeeId))
                    dto.IsAuthorized = true;

                var disposals = await _db.Disposals
                    .Include(m => m.DisposalType).Include(m => m.Asset).Include(m => m.Asset.Store)
                    .Include(m => m.Asset.SubCategory).Include(m => m.Asset.SubCategory.Category)
                .ToListAsync();

                if (IsInRole("User") || IsInRole("StoreOwner"))
                {
                    disposals = disposals.Where(m => m.EmployeeId == _employeeId).ToList();
                }

                dto.Disposals = disposals;
                dto.StoreId = _storeId;
                dto.StoreName = await _func.GetStoreNameByStoreIdAsync(_storeId);

                if (await _func.GetStoreOwnerIdAsync(_storeId) == _employeeId)
                {
                    dto.IsAuthorized = true;
                }
                else
                {
                    dto.IsAuthorized = false;
                }

                var disposalChart = await _db.Disposals
                    .Join(
                        _db.DisposalTypes,
                        disposal => disposal.DisposalTypeId,
                        disposalType => disposalType.Id,
                        (disposal, disposalType) => new { Disposal = disposal, DisposalType = disposalType }
                    )
                    .GroupBy(
                        joined => new { joined.DisposalType.Type, joined.Disposal.EmployeeId },
                        (key, grouped) => new DisposalChart
                        {
                            Type = key.Type,
                            EmployeeId = key.EmployeeId,
                            Count = grouped.Count()
                        }
                )
                .ToListAsync();

                if (IsInRole("User") || IsInRole("StoreOwner"))
                {
                    disposalChart = disposalChart.Where(m => m.EmployeeId == _employeeId).ToList();
                }
                var disposalTypes = await _db.DisposalTypes.ToListAsync();

                foreach (var type in disposalTypes)
                {
                    var chartItem = disposalChart.FirstOrDefault(m => m.Type == type.Type);
                    if (chartItem == null)
                    {
                        DisposalChart item = new DisposalChart()
                        {
                            Type = type.Type,
                            EmployeeId = _employeeId,
                            Count = 0
                        };
                        disposalChart.Add(item);
                    }
                }

                dto.ChartData = disposalChart;

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
            using var transaction = await _db.Database.BeginTransactionAsync(); // Start a new transaction
            try
            {
                _employeeId = IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;

                string sFileName = "";

                if (dto.file != null)
                {
                    // Construct the file path
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    var fileNameGuid = Guid.NewGuid().ToString();
                    var targetDir = "disposaldocuments"; // Change this to your desired directory
                    var fileExtension = Path.GetExtension(dto.file.FileName);
                    var uniqueFileName = fileNameGuid + fileExtension;
                    var filePath = Path.Combine(wwwRootPath, targetDir, uniqueFileName);

                    // Save the uploaded file to the directory
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        dto.file.CopyTo(stream);
                    }

                    sFileName = uniqueFileName;
                }

                var disposal = dto.Disposal;
                disposal.ImageUrl = sFileName;
                disposal.EmployeeId = _employeeId;

                // Add and save the disposal entity
                _db.Disposals.Add(disposal);
                await _db.SaveChangesAsync();

                // Update asset status if an asset is associated with the disposal
                var asset = await _db.Assets.FirstOrDefaultAsync(m => m.Id == disposal.AssetId);
                if (asset != null)
                {
                    asset.AssetStatusId = SD.Asset_Disposed;
                    await _db.SaveChangesAsync(); // Save changes to the asset status
                }

                var assetHistory = new AssetHistory()
                {
                    TimeStamp = DateTime.Now,
                    AssetId = disposal.AssetId,
                    TransactionRecordId = disposal.AssetId,
                    TransactionTypeId = SD.Transaction_Disposal,
                    Description = "Asset disposed by " + await _func.GetEmployeeNameAsync()
                };
                await _db.AssetHistory.AddAsync(assetHistory);
                await _db.SaveChangesAsync();

                // Log NewsFeed
                string employeeName = await _func.GetEmployeeNameAsync();
                string assetName = await _func.GetAssetNameAsync(disposal.AssetId);
                string storeName = await _func.GetStoreNameByStoreIdAsync(await _func.GetStoreIdByAssetIdAsync(disposal.AssetId));
                string message = $"{employeeName} disposed an asset ({assetName}) in {storeName}";
                await _func.LogNewsFeedAsync(message, "Users", "Disposals", "EditDisposal", disposal.AssetId);

                // Commit the transaction
                await transaction.CommitAsync();
                dto = await PopulateDisposalDtoAsync(dto);
                return Result<DisposalCreateDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                // Rollback the transaction on any error
                await transaction.RollbackAsync();
                _func.LogException(_logger, ex);
                dto = await PopulateDisposalDtoAsync(dto);
                return Result<DisposalCreateDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<DisposalEditDTO>> GetEditDisposalAsync(int id)
        {
            try
            {
                var employeeId = IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
                var storeId = await _func.GetStoreIdByEmployeeIdAsync(employeeId);


                var disposal = await _db.Disposals
                    .Where(m => m.Id == id)
                    .Include(m => m.DisposalType)
                    .Include(m => m.Asset).ThenInclude(a => a.Store)
                    .Include(m => m.Asset.SubCategory).ThenInclude(s => s.Category)
                    .FirstOrDefaultAsync();

                if (disposal == null)
                    return Result<DisposalEditDTO>.Failure("Disposal not found!");


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
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Get the supervisor ID if the user is in the "User" role
                _employeeId = IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
                string sFileName = string.Empty;

                // Handle file upload if provided
                if (dto.file != null)
                {
                    sFileName = await UploadFileAsync(dto.file);
                    if (string.IsNullOrEmpty(sFileName))
                    {
                        return Result<DisposalEditDTO>.Failure("File upload failed.");
                    }
                }

                // Fetch existing disposal from the database
                var disposalInDb = await _db.Disposals.FirstOrDefaultAsync(d => d.Id == dto.Disposal.Id);
                if (disposalInDb == null)
                {
                    return Result<DisposalEditDTO>.Failure("Record not found!");
                }

                int prevAssetId = disposalInDb.AssetId;

                // Update disposal details
                if (dto.file != null)
                {
                    dto.Disposal.ImageUrl = sFileName;
                }
                dto.Disposal.EmployeeId = _employeeId;
                _db.Entry(disposalInDb).CurrentValues.SetValues(dto.Disposal);

                // Check if AssetId has changed
                if (dto.Disposal.AssetId != prevAssetId)
                {
                    // Update previous asset status
                    await UpdateAssetStatusAsync(prevAssetId, SD.Asset_Available);
                    await AddAssetHistoryAsync(prevAssetId, "Asset un-disposed by " + await _func.GetEmployeeNameAsync());

                    // Update new asset history
                    await AddAssetHistoryAsync(dto.Disposal.AssetId, "Asset disposed by " + await _func.GetEmployeeNameAsync());
                }

                // Update current asset status if associated
                await UpdateAssetStatusAsync(dto.Disposal.AssetId, SD.Asset_Disposed);

                // Log NewsFeed
                await LogNewsFeedAsync(dto.Disposal.Id, dto.Disposal.AssetId);

                // Commit transaction
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                dto = await PopulateDisposalDtoAsync(dto);
                return Result<DisposalEditDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _func.LogException(_logger, ex);
                return Result<DisposalEditDTO>.Failure(ex.Message);
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
                        return Result<bool>.Failure("Disposal not found.");
                    }

                    int assetId = disposalInDb.AssetId;
                    var assetInDb = await _db.Assets
                        .Include(m=>m.Store)
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
                    string storeName = assetInDb?.Store?.Name ?? "-";

                    string message = $"{employeeName} un-disposed an asset ({assetName}) in {storeName}";
                    await _func.LogNewsFeedAsync(message, "Users", "Disposals", "Index", assetId);
                    await AddAssetHistoryAsync(assetId, $"Asset Un-Disposed by {employeeName}");
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

        //Populate Disposal
        public async Task<T> PopulateDisposalDtoAsync<T>(T dto) where T : class
        {
            // Check if the DTO has the necessary properties using reflection
            var isAuthorizedField = dto.GetType().GetProperty("IsAuthorized");
            var storeNameField = dto.GetType().GetProperty("StoreName");
            var storeOwnerField = dto.GetType().GetProperty("StoreOwner");
            var assetsField = dto.GetType().GetProperty("Assets");
            var disposalTypeListField = dto.GetType().GetProperty("DisposalTypeList");

            _employeeId = IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
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
                        CategoryName = a.SubCategory.Category.CategoryName,
                        SubCategoryName = a.SubCategory.SubCategoryName,
                        AssetName = a.Name,
                        Make = a.Make,
                        Model = a.Model,
                        Barcode = a.Barcode,
                        Identification = a.SubCategory.Category.CategoryName == "Vehicles" ? "Plate No: " + a.Plate : "SN: " + a.SerialNo
                    }).ToListAsync();

                assetsField.SetValue(dto, assetList);
            }

            // Populate IsAuthorized (only if the property exists)
            if (isAuthorizedField != null)
            {
                var isAuthorized = await _func.GetStoreOwnerIdAsync(_storeId) == _employeeId;
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
                    Text = m.Type,
                    Value = m.Id.ToString()
                });
                disposalTypeListField.SetValue(dto, disposalTypeList);
            }

            return dto;
        }

        //Private Functions
        private bool IsInRole(string role) => _httpContextAccessor.HttpContext.User.IsInRole(role);
        private async Task<Result<string>> DeletePictureAsync(int disposalId)
        {
            if (disposalId == 0)
            {
                return Result<string>.Failure("Disposal not found!");
            }

            var disposalInDb = await _db.Disposals.FirstOrDefaultAsync(m => m.Id == disposalId);
            if (disposalInDb == null)
            {
                return Result<string>.Failure("Disposal not found!");
            }

            // Remove file from disk
            string sFileName = disposalInDb.ImageUrl.Substring(19); // Removes "/disposaldocuments/"
            bool fileDeleted = DeleteFile(sFileName, "disposaldocuments");
            if (!fileDeleted)
            {
                return Result<string>.Failure("Error deleting picture from storage!");
            }

            return Result<string>.Success("Picture deleted successfully!");
        }
        private bool DeleteFile(string fileName, string folderName)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string folderPath = Path.Combine(wwwRootPath, folderName);
            string filePath = Path.Combine(folderPath, fileName);

            try
            {
                System.IO.File.Delete(filePath);
                return true;
            }
            catch
            {
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
            string message = $"{employeeName} modified disposal record for an asset ({assetName}) in {storeName}";
            await _func.LogNewsFeedAsync(message, "Users", "Disposals", "EditDisposal", disposalId);
        }
    }
}
