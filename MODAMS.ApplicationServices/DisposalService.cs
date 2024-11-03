using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MODAMS.ApplicationServices.IServices;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
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

        private bool IsInRole(string role) => _httpContextAccessor.HttpContext.User.IsInRole(role);

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
                    .Include(m => m.Asset)
                        .ThenInclude(a => a.Store)
                    .Include(m => m.Asset.SubCategory)
                        .ThenInclude(s => s.Category)
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

                string sFileName = "";

                // Handle file upload if provided
                if (dto.file != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    var fileNameGuid = Guid.NewGuid().ToString();
                    var targetDir = "disposaldocuments";
                    var fileExtension = Path.GetExtension(dto.file.FileName);
                    var uniqueFileName = fileNameGuid + fileExtension;
                    var filePath = Path.Combine(wwwRootPath, targetDir, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.file.CopyToAsync(stream);
                    }

                    sFileName = uniqueFileName;
                }

                var disposal = dto.Disposal;
                if (dto.file != null)
                    disposal.ImageUrl = sFileName;

                disposal.EmployeeId = _employeeId;

                // Fetch existing disposal from the database
                var disposalInDb = await _db.Disposals.FirstOrDefaultAsync(d => d.Id == dto.Disposal.Id);
                if (disposalInDb == null)
                {
                    return Result<DisposalEditDTO>.Failure("Record not found!");
                }

                int prevAssetId = disposalInDb.AssetId;
                disposal.ImageUrl = disposalInDb.ImageUrl;
                // Update the disposal in the database
                _db.Entry(disposalInDb).CurrentValues.SetValues(disposal);
                await _db.SaveChangesAsync();

                // Check if AssetId has changed
                if (disposal.AssetId != prevAssetId)
                {
                    // Un-dispose the previous asset
                    var previousAsset = await _db.Assets.FirstOrDefaultAsync(m => m.Id == prevAssetId);
                    if (previousAsset == null)
                    {
                        return Result<DisposalEditDTO>.Failure("Previous asset not found!");
                    }
                    previousAsset.AssetStatusId = SD.Asset_Available;

                    // Add history for the previous asset
                    var prevAssetHistory = new AssetHistory()
                    {
                        TimeStamp = DateTime.Now,
                        AssetId = prevAssetId,
                        TransactionRecordId = prevAssetId,
                        TransactionTypeId = SD.Transaction_Disposal,
                        Description = "Asset un-disposed by " + await _func.GetEmployeeNameAsync()
                    };
                    _db.AssetHistory.Add(prevAssetHistory);

                    // Add history for the new asset
                    var newAssetHistory = new AssetHistory()
                    {
                        TimeStamp = DateTime.Now,
                        AssetId = disposal.AssetId,
                        TransactionRecordId = disposal.AssetId,
                        TransactionTypeId = SD.Transaction_Disposal,
                        Description = "Asset disposed by " + await _func.GetEmployeeNameAsync()
                    };
                    _db.AssetHistory.Add(newAssetHistory);

                    await _db.SaveChangesAsync();
                }

                // Update asset status if the asset is associated with the disposal
                var asset = await _db.Assets.FirstOrDefaultAsync(m => m.Id == disposal.AssetId);
                if (asset != null)
                {
                    asset.AssetStatusId = SD.Asset_Disposed;
                    await _db.SaveChangesAsync();
                }

                // Log NewsFeed
                string employeeName = await _func.GetEmployeeNameAsync();
                string assetName = await _func.GetAssetNameAsync(disposal.AssetId);
                string storeName = await _func.GetStoreNameByStoreIdAsync(await _func.GetStoreIdByAssetIdAsync(disposal.AssetId));
                string message = $"{employeeName} modified disposal record for an asset ({assetName}) in {storeName}";
                await _func.LogNewsFeedAsync(message, "Users", "Disposals", "EditDisposal", disposal.Id);

                // Commit the transaction
                await transaction.CommitAsync();
                dto = await PopulateDisposalDtoAsync(dto);
                return Result<DisposalEditDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _func.LogException(_logger, ex);
                dto = await PopulateDisposalDtoAsync(dto);
                return Result<DisposalEditDTO>.Failure(ex.Message);
            }
        }



        //Populate Disposal
        public async Task<T> PopulateDisposalDtoAsync<T>(T dto) where T : class
        {
            // Check if the DTO has the necessary properties using reflection
            var employeeIdField = dto.GetType().GetProperty("IsAuthorized");
            var storeNameField = dto.GetType().GetProperty("StoreName");
            var storeOwnerField = dto.GetType().GetProperty("StoreOwner");
            var assetsField = dto.GetType().GetProperty("Assets");
            var disposalTypeListField = dto.GetType().GetProperty("DisposalTypeList");

            _employeeId = IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
            _storeId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);

            // Populate Assets
            if (assetsField != null)
            {
                var assetList = _db.Assets
                    .Where(m => m.AssetStatusId != SD.Asset_Deleted && m.StoreId == _storeId)
                    .Include(m => m.SubCategory)
                    .Include(m => m.SubCategory.Category)
                    .Where(m => m.AssetStatusId == SD.Asset_Available)
                    .ToList();

                assetsField.SetValue(dto, assetList);
            }

            // Populate IsAuthorized (only if the property exists)
            if (employeeIdField != null)
            {
                var isAuthorized = await _func.GetStoreOwnerIdAsync(_storeId) == _employeeId;
                employeeIdField.SetValue(dto, isAuthorized);
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


    }
}
