using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices
{
    public class AssetService : IAssetService
    {
        private readonly IAMSFunc _func;
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public readonly ILogger<AssetService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly int _employeeId;

        public AssetService(IAMSFunc func, ApplicationDbContext db, IHttpContextAccessor httpContextAccessor,
            ILogger<AssetService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _func = func;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            _employeeId = _func.GetEmployeeId();
            _webHostEnvironment = webHostEnvironment;
        }

        private bool IsInRole(string role) => _httpContextAccessor.HttpContext.User.IsInRole(role);

        public async Task<Result<AssetsDTO>> GetIndexAsync(int storeId, int subCategoryId = 0)
        {
            try
            {
                var assets = await _db.Assets.Where(m => m.AssetStatusId != SD.Asset_Deleted && m.StoreId == storeId).Include(m => m.AssetStatus)
                .Include(m => m.SubCategory).ThenInclude(m => m.Category)
                .Include(m => m.Condition).Include(m => m.Donor)
                .Include(m => m.Store).ToListAsync();

                if (subCategoryId > 0)
                {
                    assets = assets.Where(m => m.SubCategoryId == subCategoryId).ToList();
                }
                var categories = await _db.vwStoreCategoryAssets.Where(m => m.StoreId == storeId).Select(m => new SelectListItem
                {
                    Text = m.SubCategoryName,
                    Value = m.SubCategoryId.ToString(),
                    Selected = (m.SubCategoryId == subCategoryId)
                }).ToListAsync();

                var empId = IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;

                var dto = new AssetsDTO()
                {
                    assets = assets,
                    StoreOwnerId = await _func.GetStoreOwnerIdAsync(storeId),
                    StoreOwnerInfo = await _func.GetStoreOwnerInfoAsync(storeId),
                    CategorySelectList = categories
                };

                if (empId == await _func.GetStoreOwnerIdAsync(storeId))
                    dto.IsAuthorized = true;

                var subCategory = await _db.SubCategories.Where(m => m.Id == subCategoryId).FirstOrDefaultAsync();

                dto.SubCategoryId = 0;
                dto.SubCategoryName = "All Assets";

                if (subCategory != null)
                {
                    dto.SubCategoryId = subCategory.Id;
                    dto.SubCategoryName = subCategory.SubCategoryName;
                }

                dto.StoreId = storeId;
                dto.StoreName = await _func.GetStoreNameByStoreIdAsync(storeId);

                return Result<AssetsDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<AssetsDTO>.Failure(ex.Message);
            }

        }
        public async Task<Result<AssetListDTO>> GetAssetListAsync(int storeId)
        {
            try
            {
                var assets = await _db.Assets.Where(m => m.AssetStatusId != SD.Asset_Deleted).Include(m => m.AssetStatus)
                .Include(m => m.SubCategory).Include(m => m.Condition).Include(m => m.Donor)
                .Include(m => m.Store).ToListAsync();

                if (storeId > 0)
                {
                    assets = assets.Where(m => m.SubCategory.CategoryId == storeId).ToList();
                }
                var categories = _db.Categories.ToList().Select(m => new SelectListItem
                {
                    Text = m.CategoryName,
                    Value = m.Id.ToString(),
                    Selected = (m.Id == storeId)
                });

                var dto = new AssetListDTO()
                {
                    AssetList = assets,
                    CategorySelectList = categories,
                };
                var category = await _db.Categories.Where(m => m.Id == storeId).FirstOrDefaultAsync();

                dto.CategoryId = (category == null) ? 0 : category.Id;
                dto.CategoryName = (category == null) ? "All Assets" : category.CategoryName;

                return Result<AssetListDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<AssetListDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<AssetCreateDTO>> GetCreateAssetAsync(int storeId)
        {
            try
            {
                var dto = new AssetCreateDTO();
                dto = await PopulateDtoAssetAsync(dto);

                var empId = IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
                if (await _func.GetStoreOwnerIdAsync(storeId) == empId)
                {
                    dto.IsAuthorized = true;
                }

                dto.StoreId = storeId;
                dto.StoreName = await _func.GetStoreNameByStoreIdAsync(storeId);

                return Result<AssetCreateDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<AssetCreateDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<AssetCreateDTO>> CreateAssetAsync(AssetCreateDTO dto)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var empId = IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;

                    dto.StoreName = await _func.GetStoreNameByStoreIdAsync(dto.StoreId);

                    if (await _func.GetStoreOwnerIdAsync(dto.StoreId) != empId)
                    {
                        dto = await PopulateDtoAssetAsync(dto);
                        return Result<AssetCreateDTO>.Failure("You are not authorized to perform this action!", dto);
                    }

                    if (await _db.Assets.AnyAsync(m => m.AssetStatusId != SD.Asset_Deleted && m.SerialNo == dto.SerialNo))
                    {
                        dto = await PopulateDtoAssetAsync(dto);
                        return Result<AssetCreateDTO>.Failure("Serial Number already in use", dto);
                    }

                    if (await _db.Assets.AnyAsync(m => m.AssetStatusId != SD.Asset_Deleted && m.Barcode == dto.Barcode))
                    {
                        dto = await PopulateDtoAssetAsync(dto);
                        return Result<AssetCreateDTO>.Failure("Barcode already in use", dto);
                    }
                    if (dto == null)
                    {
                        dto = new AssetCreateDTO();
                        dto = await PopulateDtoAssetAsync(dto);
                        return Result<AssetCreateDTO>.Failure("Please fill all the mandatory fields!", dto);
                    }

                    // Create the new asset
                    var newAsset = new Asset
                    {
                        Name = dto.Name,
                        Make = dto.Make,
                        Model = dto.Model,
                        Year = dto.Year,
                        ManufacturingCountry = dto.ManufacturingCountry,
                        SerialNo = dto.SerialNo,
                        Barcode = dto.Barcode,
                        Engine = dto.Engine,
                        Chasis = dto.Chasis,
                        Plate = dto.Plate,
                        Specifications = dto.Specifications,
                        Cost = dto.Cost,
                        PurchaseDate = dto.PurchaseDate,
                        PONumber = dto.PONumber,
                        RecieptDate = dto.RecieptDate,
                        ProcuredBy = dto.ProcuredBy,
                        Remarks = dto.Remarks,
                        SubCategoryId = dto.SubCategoryId,
                        ConditionId = dto.ConditionId,
                        StoreId = dto.StoreId,
                        DonorId = dto.DonorId,
                        AssetStatusId = SD.Asset_Available
                    };

                    await _db.Assets.AddAsync(newAsset);
                    await _db.SaveChangesAsync();

                    // Log the action to the NewsFeed
                    string employeeName = await _func.GetEmployeeNameAsync();
                    string storeName = await _func.GetStoreNameByStoreIdAsync(newAsset.StoreId);
                    string message = $"{employeeName} registered a new asset ({newAsset.Name}) in {storeName}";
                    await _func.LogNewsFeedAsync(message, "Users", "Assets", "AssetInfo", newAsset.Id);

                    // Create asset history record
                    var assetHistory = new AssetHistory
                    {
                        AssetId = newAsset.Id,
                        Description = "Asset Registered by " + employeeName,
                        TimeStamp = DateTime.Now,
                        TransactionRecordId = newAsset.Id,
                        TransactionTypeId = SD.Transaction_Registration
                    };

                    await _db.AssetHistory.AddAsync(assetHistory);
                    await _db.SaveChangesAsync();

                    // Commit the transaction
                    await transaction.CommitAsync();

                    dto.Id = newAsset.Id;

                    return Result<AssetCreateDTO>.Success(dto);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _func.LogException(_logger, ex);
                    dto = await PopulateDtoAssetAsync(dto);
                    return Result<AssetCreateDTO>.Failure(ex.Message, dto);
                }
            }
        }
        public async Task<Result<AssetEditDTO>> GetEditAssetAsync(int id)
        {
            var dto = new AssetEditDTO();
            dto = await PopulateDtoAssetAsync(dto);

            try
            {
                var assetInDb = await _db.Assets
                    .Include(m => m.SubCategory)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (assetInDb == null)
                    return Result<AssetEditDTO>.Failure("Asset not found.", dto);

                dto.Id = assetInDb.Id;
                dto.CategoryId = assetInDb.SubCategory.CategoryId;
                dto.SubCategoryId = assetInDb.SubCategoryId;
                dto.Name = assetInDb.Name;
                dto.Make = assetInDb.Make;
                dto.Model = assetInDb.Model;
                dto.Year = assetInDb.Year;
                dto.Engine = assetInDb.Engine;
                dto.Chasis = assetInDb.Chasis;
                dto.Plate = assetInDb.Plate;
                dto.ManufacturingCountry = assetInDb.ManufacturingCountry;
                dto.SerialNo = assetInDb.SerialNo;
                dto.Barcode = assetInDb.Barcode;
                dto.Specifications = assetInDb.Specifications;
                dto.Cost = assetInDb.Cost;
                dto.PurchaseDate = assetInDb.PurchaseDate;
                dto.RecieptDate = assetInDb.RecieptDate;
                dto.PONumber = assetInDb.PONumber;
                dto.ProcuredBy = assetInDb.ProcuredBy;
                dto.DonorId = assetInDb.DonorId;
                dto.ConditionId = assetInDb.ConditionId;
                dto.Remarks = assetInDb.Remarks;
                dto.AssetStatusId = assetInDb.AssetStatusId;
                dto.StoreId = assetInDb.StoreId;

                // Get store name and assign it to the DTO
                dto.StoreName = await _func.GetStoreNameByStoreIdAsync(assetInDb.StoreId);
                return Result<AssetEditDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<AssetEditDTO>.Failure(ex.Message, dto);
            }

        }
        public async Task<Result<AssetEditDTO>> EditAssetAsync(AssetEditDTO dto)
        {
            try
            {
                var assetInDb = await _db.Assets
                    .Where(m => m.Id == dto.Id).FirstOrDefaultAsync();

                // Check if the record is available
                if (assetInDb == null)
                {
                    // Populate select lists before returning the DTO
                    dto = await PopulateDtoAssetAsync(dto);
                    return Result<AssetEditDTO>.Failure("Asset not found!", dto);
                }

                // Check for existing serial number
                var recordWithSameSerialNo = await _db.Assets
                    .Where(m => m.Id != dto.Id && m.SerialNo == dto.SerialNo)
                    .FirstOrDefaultAsync();

                if (dto.SerialNo != "-")
                {
                    if (recordWithSameSerialNo != null)
                    {
                        // Populate select lists before returning the DTO
                        dto = await PopulateDtoAssetAsync(dto);
                        return Result<AssetEditDTO>.Failure($"Serial number already assigned to Asset with Id {recordWithSameSerialNo.Id}!", dto);
                    }
                }
                // Update the record
                assetInDb.SubCategoryId = dto.SubCategoryId;
                assetInDb.Name = dto.Name;
                assetInDb.Make = dto.Make;
                assetInDb.Model = dto.Model;
                assetInDb.Year = dto.Year;
                assetInDb.Engine = dto.Engine;
                assetInDb.Chasis = dto.Chasis;
                assetInDb.Plate = dto.Plate;
                assetInDb.ManufacturingCountry = dto.ManufacturingCountry;
                assetInDb.SerialNo = dto.SerialNo;
                assetInDb.Barcode = dto.Barcode;
                assetInDb.Specifications = dto.Specifications;
                assetInDb.Cost = dto.Cost;
                assetInDb.PurchaseDate = dto.PurchaseDate;
                assetInDb.RecieptDate = dto.RecieptDate;
                assetInDb.PONumber = dto.PONumber;
                assetInDb.ProcuredBy = dto.ProcuredBy;
                assetInDb.DonorId = dto.DonorId;
                assetInDb.ConditionId = dto.ConditionId;
                assetInDb.Remarks = dto.Remarks;
                assetInDb.AssetStatusId = dto.AssetStatusId;

                await _db.SaveChangesAsync();

                // Log Newsfeed
                string employeeName = await _func.GetEmployeeNameAsync();
                string assetName = assetInDb.Name;
                string storeName = await _func.GetStoreNameByStoreIdAsync(assetInDb.StoreId);
                string message = $"{employeeName} modified an asset ({assetName}) in {storeName}";
                await _func.LogNewsFeedAsync(message, "Users", "Assets", "AssetInfo", assetInDb.Id);

                // Return success with populated DTO
                dto.StoreName = await _func.GetStoreNameByStoreIdAsync(dto.StoreId);
                dto = await PopulateDtoAssetAsync(dto);
                return Result<AssetEditDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);

                // Populate select lists and return the DTO on failure
                dto = await PopulateDtoAssetAsync(dto);
                dto.StoreName = await _func.GetStoreNameByStoreIdAsync(dto.StoreId);
                return Result<AssetEditDTO>.Failure(ex.Message, dto);
            }
        }
        public async Task<Result<AssetDocumentDTO>> GetAssetDocumentsAsync(int assetId)
        {
            try
            {
                var dto = new AssetDocumentDTO();
                dto.DocumentList = await GetDocumentListAsync(assetId);


                var asset = await _db.Assets.Where(m => m.Id == assetId).FirstOrDefaultAsync();
                if (asset != null)
                    dto.AssetInfo = asset.Name + " - " + asset.Model + " - " + asset.Year;

                int storeId = await _func.GetStoreIdByAssetIdAsync(assetId);
                string storeName = await _func.GetStoreNameByStoreIdAsync(storeId);

                dto.StoreId = storeId;
                dto.StoreName = storeName;
                dto.AssetId = assetId;

                return Result<AssetDocumentDTO>.Success(dto);

            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<AssetDocumentDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result> UploadDocumentAsync(int assetId, int documentTypeId, IFormFile? file)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if (file != null && assetId > 0 && documentTypeId > 0)
            {
                string sFileName = "";
                string sPath = "";

                var documentType = await _db.DocumentTypes.FirstOrDefaultAsync(m => m.Id == documentTypeId);
                if (documentType != null)
                {
                    sFileName = documentType.Name;
                }

                string fileName = sFileName + Path.GetExtension(file.FileName);
                sPath = Path.Combine(wwwRootPath, @"assetdocuments");

                try
                {
                    // Save the file to the server
                    using (var fileStream = new FileStream(Path.Combine(sPath, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    // Remove old document if it exists
                    var assetDocument = await _db.AssetDocuments
                        .FirstOrDefaultAsync(m => m.AssetId == assetId && m.DocumentTypeId == documentTypeId);
                    if (assetDocument != null)
                    {
                        _db.AssetDocuments.Remove(assetDocument);
                        await _db.SaveChangesAsync();
                    }

                    // Add the new document record
                    var newAssetDocument = new AssetDocument
                    {
                        Name = sFileName,
                        DocumentUrl = "/assetdocuments/" + fileName,
                        DocumentTypeId = documentTypeId,
                        AssetId = assetId,
                    };
                    await _db.AssetDocuments.AddAsync(newAssetDocument);
                    await _db.SaveChangesAsync();

                    // Log the news feed
                    string employeeName = await _func.GetEmployeeNameAsync();
                    string assetName = await _func.GetAssetNameAsync(assetId);
                    string storeName = await _func.GetStoreNameByStoreIdAsync(await _func.GetStoreIdByAssetIdAsync(assetId));
                    string message = $"{employeeName} uploaded {sFileName} for ({assetName}) in {storeName}";
                    await _func.LogNewsFeedAsync(message, "Users", "Assets", "AssetInfo", assetId);

                    return Result.Success();
                }
                catch (Exception ex)
                {
                    _func.LogException(_logger, ex);
                    return Result.Failure("Error occurred: " + ex.Message);
                }
            }

            return Result.Failure("Please select a valid file and try again.");
        }
        public async Task<Result<AssetInfoDTO>> GetAssetInfoAsync(int id, int page = 1, int tab = 1, int categoryId = 0)
        {
            var dto = new AssetInfoDTO();

            try
            {
                // Fetch asset details
                var asset = await _db.Assets.Where(m => m.Id == id)
                    .Include(m => m.SubCategory)
                    .Include(m => m.Condition)
                    .Include(m => m.SubCategory.Category)
                    .Include(m => m.AssetStatus)
                    .Include(m => m.Store)
                    .Include(m => m.Donor)
                    .FirstOrDefaultAsync();

                if (asset == null)
                {
                    return Result<AssetInfoDTO>.Failure("Record not found!");
                }

                // Fetch asset documents
                var documents = await _db.AssetDocuments.Where(m => m.AssetId == id).ToListAsync();

                // Fetch asset pictures
                var assetPictures = await _db.AssetPictures.Where(m => m.AssetId == id).ToListAsync();
                var dtoAssetPictures = new AssetPicturesDTO(assetPictures, 6);

                // Fetch asset history
                var assetHistory = await _db.AssetHistory.Where(m => m.AssetId == id)
                    .OrderBy(m => m.TimeStamp)
                    .ToListAsync();

                // Populate DTO
                dto.Asset = asset;
                dto.Documents = documents;
                dto.dtoAssetPictures = dtoAssetPictures;
                dto.AssetHistory = assetHistory;

                return Result<AssetInfoDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<AssetInfoDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<DeleteDocumentResultDTO>> DeleteDocumentAsync(int documentId)
        {
            try
            {
                if (documentId <= 0)
                {
                    return Result<DeleteDocumentResultDTO>.Failure("Invalid document ID.");
                }

                var assetDocument = await _db.AssetDocuments.FirstOrDefaultAsync(m => m.Id == documentId);
                if (assetDocument == null)
                {
                    return Result<DeleteDocumentResultDTO>.Failure("Document not found!");
                }

                // Extract the file name from the URL
                var sFileName = assetDocument.DocumentUrl.Substring(16);

                // Attempt to delete the file
                if (!DeleteFile(sFileName, "assetdocuments"))
                {
                    return Result<DeleteDocumentResultDTO>.Failure("Error deleting file from storage");
                }

                // Remove the document from the database
                _db.AssetDocuments.Remove(assetDocument);
                await _db.SaveChangesAsync();

                int assetId = assetDocument.AssetId;
                int storeId = await _func.GetStoreIdByAssetIdAsync(assetId);

                var resultDto = new DeleteDocumentResultDTO()
                {
                    AssetId = assetId,
                    StoreId = storeId,
                };

                // Return success with the asset and store info to update TempData in the controller
                return Result<DeleteDocumentResultDTO>.Success(resultDto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<DeleteDocumentResultDTO>.Failure(ex.Message);
            }

        }
        public async Task<Result<AssetPicturesDTO>> GetAssetPicturesAsync(int assetId)
        {
            var assetPictures = await _db.AssetPictures.Where(m => m.AssetId == assetId).ToListAsync();
            var dto = new AssetPicturesDTO(assetPictures, 6);

            try
            {
                var storeId = await _func.GetStoreIdByAssetIdAsync(assetId);
                var storeName = await _func.GetStoreNameByStoreIdAsync(storeId);

                dto.StoreName = storeName;
                dto.StoreId = storeId;
                dto.AssetId = assetId;

                var asset = await _db.Assets.Where(m => m.Id == assetId).FirstOrDefaultAsync();
                if (asset != null)
                {
                    dto.AssetInfo = asset.Name + " - " + asset.Model + " - " + asset.Year;
                }
                return Result<AssetPicturesDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<AssetPicturesDTO>.Failure(ex.Message, dto);
            }
        }
        public async Task<Result<string>> UploadPictureAsync(int assetId, IFormFile? file)
        {
            if (file == null)
            {
                return Result<string>.Failure("File not available");
            }

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            Guid guid = Guid.NewGuid();
            string fileName = guid + Path.GetExtension(file.FileName);
            string path = Path.Combine(wwwRootPath, "assetpictures");

            try
            {
                // Save the file to disk
                using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Save the asset picture to the database
                var assetPicture = new AssetPicture
                {
                    ImageUrl = "/assetpictures/" + fileName,
                    AssetId = assetId
                };
                await _db.AssetPictures.AddAsync(assetPicture);
                await _db.SaveChangesAsync();

                // Log the action in the news feed
                string employeeName = await _func.GetEmployeeNameAsync();
                string assetName = await _func.GetAssetNameAsync(assetId);
                string storeName = await _func.GetStoreNameByStoreIdAsync(await _func.GetStoreIdByAssetIdAsync(assetId));
                string message = $"{employeeName} uploaded a picture for ({assetName}) in {storeName}";
                await _func.LogNewsFeedAsync(message, "Users", "Assets", "AssetInfo", assetId);

                return Result<string>.Success("Picture uploaded successfully!");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(ex.Message);
            }
        }
        public async Task<Result<string>> DeletePictureAsync(int id, int assetId)
        {
            if (id == 0)
            {
                return Result<string>.Failure("Picture not found!");
            }

            var assetPicture = await _db.AssetPictures.FirstOrDefaultAsync(m => m.Id == id);
            if (assetPicture == null)
            {
                return Result<string>.Failure("Picture not found!");
            }

            // Remove file from disk
            string sFileName = assetPicture.ImageUrl.Substring(15); // Removes "/assetpictures/"
            bool fileDeleted = DeleteFile(sFileName, "assetpictures");
            if (!fileDeleted)
            {
                return Result<string>.Failure("Error deleting picture from storage!");
            }

            // Remove from database
            _db.AssetPictures.Remove(assetPicture);
            await _db.SaveChangesAsync();

            return Result<string>.Success("Picture deleted successfully!");
        }
        public async Task<Result<string>> DeleteAssetAsync(int assetId)
        {
            if (assetId == 0)
            {
                return Result<string>.Failure("Asset not found!");
            }

            var assetInDb = await _db.Assets.FirstOrDefaultAsync(m => m.Id == assetId);
            if (assetInDb == null)
            {
                return Result<string>.Failure("Asset not found!");
            }

            // Mark asset as deleted
            assetInDb.AssetStatusId = SD.Asset_Deleted;
            assetInDb.Remarks = $"Asset Deleted by {await _func.GetEmployeeNameAsync()}";
            await _db.SaveChangesAsync();

            // Add to asset history
            var assetHistory = new AssetHistory
            {
                AssetId = assetId,
                Description = $"Asset Deleted by {await _func.GetEmployeeNameAsync()}",
                TimeStamp = DateTime.Now,
                TransactionRecordId = assetId,
                TransactionTypeId = SD.Transaction_Delete
            };
            _db.AssetHistory.Add(assetHistory);
            await _db.SaveChangesAsync();
            
            return Result<string>.Success("Asset deleted successfully!");
        }
        public async Task<Result<string>> RecoverAssetAsync(int assetId)
        {
            if (assetId == 0)
            {
                return Result<string>.Failure("Asset not found!");
            }

            var assetInDb = await _db.Assets.FirstOrDefaultAsync(m => m.Id == assetId);
            if (assetInDb == null)
            {
                return Result<string>.Failure("Asset not found!");
            }
                        
            assetInDb.AssetStatusId = SD.Asset_Available; // Will have to change it to the previous Asset Status ID Later
            assetInDb.Remarks = $"Asset Recovered by {await _func.GetEmployeeNameAsync()}";
            await _db.SaveChangesAsync();

            var assetHistory = new AssetHistory
            {
                AssetId = assetId,
                Description = $"Asset Recovered by {await _func.GetEmployeeNameAsync()}",
                TimeStamp = DateTime.Now,
                TransactionRecordId = assetId,
                TransactionTypeId = SD.Transaction_Recover
            };
            _db.AssetHistory.Add(assetHistory);
            await _db.SaveChangesAsync();

            return Result<string>.Success("Asset recovered successfully!");
        }

        //Populate Asset DTO
        public async Task<T> PopulateDtoAssetAsync<T>(T dto) where T : class, new()
        {
            // Use reflection to populate common fields for both DTOs
            var categories = await _db.Categories
                .Select(m => new SelectListItem
                {
                    Text = m.CategoryName,
                    Value = m.Id.ToString()
                })
                .ToListAsync();

            var subCategories = await _db.SubCategories
                .Select(m => new SelectListItem
                {
                    Text = m.SubCategoryName,
                    Value = m.Id.ToString()
                })
                .ToListAsync();

            var donors = await _db.Donors
                .Select(m => new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString()
                })
                .ToListAsync();

            var statuses = await _db.AssetStatuses
                .Select(m => new SelectListItem
                {
                    Text = m.StatusName,
                    Value = m.Id.ToString()
                })
                .ToListAsync();

            var conditions = await _db.Conditions
                .Select(m => new SelectListItem
                {
                    Text = m.ConditionName,
                    Value = m.Id.ToString()
                })
                .ToListAsync();

            // Check the type of DTO and assign properties accordingly
            if (dto is AssetCreateDTO createDto)
            {
                createDto.Categories = categories;
                createDto.SubCategories = subCategories;
                createDto.Donors = donors;
                createDto.Statuses = statuses;
                createDto.Conditions = conditions;
            }
            else if (dto is AssetEditDTO editDto)
            {
                editDto.Categories = categories;
                editDto.SubCategories = subCategories;
                editDto.Donors = donors;
                editDto.Statuses = statuses;
                editDto.Conditions = conditions;
            }

            return dto;
        }

        //API Calls
        public async Task<Result<string>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _db.Categories.ToListAsync();
                return Result<string>.Success(JsonConvert.SerializeObject(categories));
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<string>.Failure(ex.Message);
            }
        }
        public async Task<Result<string>> GetSubCategoriesAsync(int? categoryId)
        {
            string result = "Data not available!";
            try
            {
                var subCategories = await _db.SubCategories.ToListAsync();

                if (categoryId != null)
                    subCategories = subCategories.Where(m => m.CategoryId == categoryId).ToList();

                if (subCategories.Count > 0)
                {
                    result = JsonConvert.SerializeObject(subCategories);
                }
                return Result<string>.Success(result);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<string>.Failure(ex.Message);
            }
        }
        public async Task<Result<string>> GetDocumentTypesAsync()
        {
            try
            {
                string result = "Data not available!";

                var documentTypes = await _db.DocumentTypes.ToListAsync();
                if (documentTypes != null)
                {
                    result = JsonConvert.SerializeObject(documentTypes);
                }
                return Result<string>.Success(result);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<string>.Failure(ex.Message);
            }
        }

        //Private functions
        private async Task<List<vwAssetDocument>> GetDocumentListAsync(int AssetId)
        {

            var documentTypes = await _db.DocumentTypes.ToListAsync();
            var documentList = new List<vwAssetDocument>();

            foreach (var documentType in documentTypes)
            {
                var vwDocType = new vwAssetDocument()
                {
                    Id = GetAssetDocumentId(AssetId, documentType.Id),
                    DocumentTypeId = documentType.Id,
                    Name = documentType.Name,
                    AssetId = AssetId,
                    DocumentUrl = GetDocumentUrl(AssetId, documentType.Id)
                };
                documentList.Add(vwDocType);
            }

            return documentList;
        }
        private string GetDocumentUrl(int assetId, int documentTypeId)
        {
            var assetDocument = _db.AssetDocuments.Where(m => m.AssetId == assetId && m.DocumentTypeId == documentTypeId).FirstOrDefault();
            if (assetDocument != null)
            {
                return assetDocument.DocumentUrl;
            }
            else
            {
                return "not yet uploaded!";
            }

        }
        private int GetAssetDocumentId(int assetId, int documentTypeId)
        {
            int nResult = 0;
            var assetDocument = _db.AssetDocuments.Where(m => m.AssetId == assetId && m.DocumentTypeId == documentTypeId).FirstOrDefault();
            if (assetDocument != null)
            {
                nResult = assetDocument.Id;
            }
            return nResult;
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
    }
}
