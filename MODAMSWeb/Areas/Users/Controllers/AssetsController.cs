using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using Newtonsoft.Json;
using NuGet.ContentModel;
using NuGet.Protocol;
using System.Data;
using System.Drawing;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AssetsController(ApplicationDbContext db, IAMSFunc func, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(int id, int subCategoryId = 0)
        {
            var assets = _db.Assets.Where(m => m.StoreId == id).Include(m => m.AssetStatus)
                .Include(m => m.SubCategory).Include(m => m.Condition).Include(m => m.Donor)
                .Include(m => m.Store).ToList();

            if (subCategoryId > 0)
            {
                assets = assets.Where(m => m.SubCategoryId == subCategoryId).ToList();
            }
            var categories = _db.vwStoreCategoryAssets.Where(m => m.StoreId == id).ToList().Select(m=> new SelectListItem {
                Text = m.SubCategoryName,
                Value = m.SubCategoryId.ToString(),
                Selected = (m.SubCategoryId == subCategoryId)
            });

            var empId = User.IsInRole("User") ? _func.GetSupervisorId(_employeeId) : _employeeId;

            var dto = new dtoAssets()
            {
                assets = assets,
                StoreOwnerId = _func.GetStoreOwnerId(id),
                 CategorySelectList = categories
            };

            if (empId == _func.GetStoreOwnerId(id))
                dto.IsAuthorized = true;

            var subCategory = _db.SubCategories.Where(m => m.Id == subCategoryId).FirstOrDefault();

            TempData["SubCategoryId"] = 0;
            TempData["SubCategoryName"] = "All Assets";

            if (subCategory != null)
            {
                TempData["SubCategoryId"] = subCategory.Id;
                TempData["SubCategoryName"] = subCategory.SubCategoryName;
            }

            TempData["storeId"] = id;
            TempData["storeName"] = _func.GetStoreNameByStoreId(id);


            return View(dto);
        }

        public IActionResult AssetList(int id)
        {

            var assets = _db.Assets.Include(m => m.AssetStatus)
                .Include(m => m.SubCategory).Include(m => m.Condition).Include(m => m.Donor)
                .Include(m => m.Store).ToList();

            if (id > 0)
            {
                assets = assets.Where(m => m.SubCategory.CategoryId == id).ToList();
            }
            var categories = _db.Categories.ToList().Select(m => new SelectListItem
            {
                Text = m.CategoryName,
                Value = m.Id.ToString(),
                Selected = (m.Id == id)
            });

            var dto = new dtoAssetList()
            {
                CategoryId = id,
                AssetList = assets,
                CategorySelectList = categories,
            };
            var category = _db.Categories.Where(m => m.Id == id).FirstOrDefault();
            TempData["categoryId"] = 0;
            TempData["categoryName"] = "All Assets";

            if (category != null) {
                TempData["categoryId"] = category.Id;
                TempData["categoryName"] = category.CategoryName;
            }
            
            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpGet]
        public IActionResult CreateAsset(int id)
        {
            var empId = User.IsInRole("User") ? _func.GetSupervisorId(_employeeId) : _employeeId;
            if (_func.GetStoreOwnerId(id) != empId)
            {
                TempData["error"] = "You are not authorized to perform this action!";
                return RedirectToAction("Index", "Assets", new { area = "Users", id = id });
            }

            var dto = new dtoAsset();
            dto = PopulateDtoAsset(dto);

            TempData["storeId"] = id;
            TempData["storeName"] = _func.GetStoreNameByStoreId(id);



            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsset(dtoAsset dto)
        {
            var empId = User.IsInRole("User") ? _func.GetSupervisorId(_employeeId) : _employeeId;
            if (_func.GetStoreOwnerId(dto.StoreId) != empId)
            {
                TempData["error"] = "You are not authorized to perform this action!";
                return RedirectToAction("Index", "Assets", new { area = "Users", id = dto.StoreId });
            }

            if (ModelState.IsValid)
            {
                var asset = _db.Assets.Where(m => m.SerialNo == dto.SerialNo).FirstOrDefault();
                if (asset == null)
                {
                    var newAsset = new MODAMS.Models.Asset()
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
                        AssetStatusId = 1
                    };

                    await _db.Assets.AddAsync(newAsset);
                    await _db.SaveChangesAsync();
                    TempData["success"] = "Asset added successfuly!";
                    return RedirectToAction("Index", "Assets", new { area = "Users", id = dto.StoreId });
                }
                else
                {
                    TempData["error"] = "Serial Number already in use";

                    TempData["storeId"] = dto.StoreId;
                    TempData["storeName"] = _func.GetStoreNameByStoreId(dto.StoreId);
                    dto = PopulateDtoAsset(dto);

                    return View(dto);
                }
            }
            else
            {
                TempData["error"] = "Please fill all the mandatory fields!";

                TempData["storeId"] = dto.StoreId;
                TempData["storeName"] = _func.GetStoreNameByStoreId(dto.StoreId);

                dto = PopulateDtoAsset(dto);
                return View(dto);
                //return RedirectToAction("CreateAsset", "Assets", new { area = "Users", id = dto.StoreId });
            }
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpGet]
        public IActionResult EditAsset(int id)
        {
            var dto = new dtoAsset();
            dto = PopulateDtoAsset(dto);

            if (id > 0)
            {
                var assetInDb = _db.Assets.Where(m => m.Id == id).FirstOrDefault();
                if (assetInDb != null)
                {
                    dto.Id = assetInDb.Id;
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

                    TempData["storeId"] = assetInDb.StoreId;
                    TempData["storeName"] = _func.GetStoreNameByStoreId(assetInDb.StoreId);

                }
            }

            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsset(dtoAsset dto)
        {
            var empId = User.IsInRole("User") ? _func.GetSupervisorId(_employeeId) : _employeeId;
            if (_func.GetStoreOwnerId(dto.StoreId) != empId)
            {
                TempData["error"] = "You are not authorized to perform this action!";
                return RedirectToAction("Index", "Assets", new { area = "Users", id = dto.StoreId });
            }

            if (ModelState.IsValid)
            {
                var assetInDb = await _db.Assets.Where(m => m.Id == dto.Id).FirstOrDefaultAsync();
                if (assetInDb != null)
                {
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

                    TempData["success"] = "Changes saved succesfuly!";
                    return RedirectToAction("EditAsset", "Assets", new { area = "Users", id = dto.Id });
                }
                else
                {
                    dto = PopulateDtoAsset(dto);
                    TempData["error"] = "Record not found!";
                    TempData["storeId"] = dto.StoreId;
                    TempData["storeName"] = _func.GetStoreNameByStoreId(dto.StoreId);
                    return View(dto);
                }
            }
            else
            {
                dto = PopulateDtoAsset(dto);
                TempData["error"] = "All fields are mandatory!";
                TempData["storeId"] = dto.StoreId;
                TempData["storeName"] = _func.GetStoreNameByStoreId(dto.StoreId);
                return View(dto);
            }
        }

        [Authorize(Roles = "Administrator, StoreOwner, User")]
        [HttpGet]
        public IActionResult AssetDocuments(int id)
        {
            var dto = new dtoAssetDocument();
            dto = PopulateDtoAssetDocument(dto, id);

            var asset = _db.Assets.Where(m => m.Id == id).FirstOrDefault();
            if (asset != null)
            {
                TempData["assetInfo"] = asset.Name + " - " + asset.Model + " - " + asset.Year;
            }

            int nStoreId = _func.GetStoreIdByAssetId(id);
            string sStoreName = _func.GetStoreNameByStoreId(nStoreId);

            TempData["storeId"] = nStoreId;
            TempData["storeName"] = sStoreName;

            return View(dto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int Id, int DocumentTypeId, IFormFile? file)
        {

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null && Id > 0 && DocumentTypeId > 0)
            {
                string sFileName = "";
                string sPath = "";

                var documentType = _db.DocumentTypes.Where(m => m.Id == DocumentTypeId).FirstOrDefault();
                if (documentType != null)
                {
                    sFileName = documentType.Name;
                }

                string fileName = sFileName + Path.GetExtension(file.FileName);
                sPath = Path.Combine(wwwRootPath, @"assetdocuments");

                try
                {
                    using (var fileStream = new FileStream(Path.Combine(sPath, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    var assetDocument = _db.AssetDocuments.Where(m => m.AssetId == Id && m.DocumentTypeId == DocumentTypeId).FirstOrDefault();
                    if (assetDocument != null)
                    {
                        _db.AssetDocuments.Remove(assetDocument);
                        await _db.SaveChangesAsync();
                    }

                    var newAssetDocument = new AssetDocument()
                    {
                        Name = sFileName,
                        DocumentUrl = "/assetdocuments/" + fileName,
                        DocumentTypeId = DocumentTypeId,
                        AssetId = Id,
                    };
                    await _db.AssetDocuments.AddAsync(newAssetDocument);
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Error occured! <br /> " + ex.Message;
                    return RedirectToAction("AssetDocuments", "Assets", new { area = "Users", id = Id });
                }
                TempData["success"] = "File uploaded successfuly!";
                return RedirectToAction("AssetDocuments", "Assets", new { area = "Users", id = Id });
            }
            else
            {
                TempData["error"] = "Please select a file to upload!";
                return RedirectToAction("AssetDocuments", "Assets", new { area = "Users", id = Id });
            }
        }

        [HttpGet]
        public IActionResult AssetInfo(int id, int page = 1, int tab = 1, int categoryId = 0)
        {
            var dto = new dtoAssetInfo();

            var asset = _db.Assets.Where(m => m.Id == id)
                .Include(m => m.SubCategory)
                .Include(m => m.Condition)
                .Include(m => m.SubCategory.Category)
                .Include(m => m.AssetStatus)
                .Include(m => m.Store).Include(m => m.Donor)
                .FirstOrDefault();

            var documents = _db.AssetDocuments.Where(m => m.AssetId == id).ToList();

            TempData["categoryId"] = categoryId;

            dto.Documents = documents;
            if (asset != null)
            {
                TempData["storeId"] = asset.StoreId;
                TempData["storeName"] = asset.Store.Name;
                dto.Asset = asset;
            }
            else
            {
                TempData["error"] = "Record not found!";
            }

            var assetPictures = _db.AssetPictures.Where(m => m.AssetId == id).ToList();

            var dtoAssetPictures = new dtoAssetPictures(assetPictures, 6);

            TempData["tab"] = tab.ToString();

            dto.dtoAssetPictures = dtoAssetPictures;

            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            if (id > 0)
            {
                var assetDocument = await _db.AssetDocuments.Where(m => m.Id == id).FirstOrDefaultAsync();
                if (assetDocument != null)
                {

                    var sFileName = assetDocument.DocumentUrl.ToString();
                    sFileName = sFileName.Substring(16, sFileName.Length - 16);

                    if (!DeleteFile(sFileName, "assetdocuments"))
                    {
                        TempData["error"] = "Error deleting file from storage";
                        return RedirectToAction("AssetDocuments", "Assets", new { id = assetDocument.AssetId });
                    }

                    _db.AssetDocuments.Remove(assetDocument);
                    await _db.SaveChangesAsync();

                    int nAssetId = assetDocument.AssetId;
                    int nStoreId = _func.GetStoreIdByAssetId(nAssetId);


                    TempData["storeId"] = _func.GetStoreIdByAssetId(nAssetId);
                    TempData["storeName"] = _func.GetStoreNameByStoreId(nStoreId);

                    return RedirectToAction("AssetDocuments", "Assets", new { id = nAssetId });

                }
                else
                {
                    TempData["error"] = "Document not found!";
                    return View();
                }
            }
            else
            {
                TempData["error"] = "Document not found!";
                return View();
            }
        }

        [HttpGet]
        public IActionResult AssetPictures(int id)
        {
            var assetPictures = _db.AssetPictures.Where(m => m.AssetId == id).ToList();

            var storeId = _func.GetStoreIdByAssetId(id);
            var storeName = _func.GetStoreNameByStoreId(storeId);

            var asset = _db.Assets.Where(m => m.Id == id).FirstOrDefault();
            if (asset != null)
            {
                TempData["assetInfo"] = asset.Name + " - " + asset.Model + " - " + asset.Year;
            }

            TempData["storeId"] = storeId;
            TempData["storeName"] = storeName;
            TempData["assetId"] = id;

            return View(assetPictures);
        }

        [HttpPost]
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> UploadPicture(int AssetId, IFormFile? file)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                Guid guid = Guid.NewGuid();
                string fileName = guid + Path.GetExtension(file.FileName);
                string path = Path.Combine(wwwRootPath, @"assetpictures");

                try
                {
                    using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    var assetPicture = new AssetPicture()
                    {
                        ImageUrl = "/assetpictures/" + fileName,
                        AssetId = AssetId
                    };
                    await _db.AssetPictures.AddAsync(assetPicture);
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Error occured! <br /> " + ex.Message;
                    return RedirectToAction("AssetPictures", "Assets", new { area = "Users", id = AssetId });
                }
                finally
                {
                    TempData["success"] = "Picture uploaded successfuly!";
                }
            }
            else
            {
                TempData["error"] = "Error uploading file!";
                return RedirectToAction("AssetPictures", "Assets", new { area = "Users", id = AssetId });
            }

            return RedirectToAction("AssetPictures", "Assets", new { area = "Users", id = AssetId });
        }

        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> DeletePicture(int id, int assetId)
        {
            bool blnInitialValidation = true;
            if (id == 0)
            {
                TempData["error"] = "Picture not found!";
                blnInitialValidation = false;
            }

            string sFileName = "";

            var assetPicture = await _db.AssetPictures.Where(m => m.Id == id).FirstOrDefaultAsync();
            if (assetPicture == null)
            {
                TempData["error"] = "Picture not found!";
                blnInitialValidation = false;
            }
            else
            {
                sFileName = assetPicture.ImageUrl.ToString();
                sFileName = sFileName.Substring(15, sFileName.Length - 15);

                _db.AssetPictures.Remove(assetPicture);
                await _db.SaveChangesAsync();
            }

            if (!blnInitialValidation)
            {
                return RedirectToAction("AssetPictures", "Assets", new { area = "Users", id = assetId });
            }

            if (!DeleteFile(sFileName, "assetpictures"))
            {
                TempData["error"] = "Error deleting picture from storage!";
                return RedirectToAction("AssetPictures", "Assets", new { area = "Users", id = assetId });
            }

            TempData["success"] = "Picture deleted successfuly!";
            return RedirectToAction("AssetPictures", "Assets", new { area = "Users", id = assetId });
        }

        //API Calls
        [HttpGet]
        public async Task<string> GetCategories()
        {
            string sResult = "No Records Found";
            var categories = await _db.Categories.ToListAsync();

            if (categories != null)
            {
                sResult = JsonConvert.SerializeObject(categories);
            }

            return sResult;
        }

        [HttpGet]
        public async Task<string> GetSubCategories(int? id)
        {
            string sResult = "No Records Found";
            var subCategories = await _db.SubCategories.ToListAsync();

            if (id != null)
                subCategories = subCategories.Where(m => m.CategoryId == id).ToList();

            if (subCategories != null)
            {
                sResult = JsonConvert.SerializeObject(subCategories);
            }

            return sResult;
        }

        [HttpGet]
        public async Task<string> GetDocumentTypes()
        {
            string sResult = "No Records Found";

            var documentTypes = await _db.DocumentTypes.ToListAsync();
            if (documentTypes != null)
            {
                sResult = JsonConvert.SerializeObject(documentTypes);
            }
            return sResult;
        }
        //API Calls End



        //Private functions
        private dtoAsset PopulateDtoAsset(dtoAsset dto)
        {
            var categories = _db.Categories.ToList().Select(m => new SelectListItem
            {
                Text = m.CategoryName,
                Value = m.Id.ToString()
            });
            var subCategories = _db.SubCategories.ToList().Select(m => new SelectListItem
            {
                Text = m.SubCategoryName,
                Value = m.Id.ToString()
            });
            var donors = _db.Donors.ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            });
            var statuses = _db.AssetStatuses.ToList().Select(m => new SelectListItem
            {
                Text = m.StatusName,
                Value = m.Id.ToString()
            });
            var conditions = _db.Conditions.ToList().Select(m => new SelectListItem
            {
                Text = m.ConditionName,
                Value = m.Id.ToString()
            });

            dto.Categories = categories;
            dto.SubCategories = subCategories;
            dto.Donors = donors;
            dto.Statuses = statuses;
            dto.Conditions = conditions;

            return dto;

        }
        private dtoAssetDocument PopulateDtoAssetDocument(dtoAssetDocument dto, int AssetId)
        {

            var documentTypes = _db.DocumentTypes.ToList();
            var vwAssetDocs = new List<vwAssetDocuments>();

            foreach (var documentType in documentTypes)
            {
                var vwDocType = new vwAssetDocuments()
                {
                    Id = GetAssetDocumentId(AssetId, documentType.Id),
                    DocumentTypeId = documentType.Id,
                    Name = documentType.Name,
                    AssetId = AssetId,
                    DocumentUrl = GetDocumentUrl(AssetId, documentType.Id)
                };
                vwAssetDocs.Add(vwDocType);
            }
            dto.vwAssetDocuments = vwAssetDocs;

            return dto;
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

            bool blnResult = true;
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch
            {
                blnResult = false;
            }
            return blnResult;
        }
    }
}