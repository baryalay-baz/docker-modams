using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Utility;
using Newtonsoft.Json;
using System.Data;


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

        public IActionResult Index(int id)
        {
            var assets = _db.Assets.Where(m => m.StoreId == id).Include(m => m.AssetStatus)
                .Include(m => m.SubCategory).Include(m => m.Condition).Include(m => m.Donor)
                .Include(m => m.Store).ToList();
            TempData["storeId"] = id;
            TempData["storeName"] = _func.GetStoreName(id);

            return View(assets);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpGet]
        public IActionResult CreateAsset(int id)
        {
            var dto = new dtoAsset();
            dto = PopulateDtoAsset(dto);

            TempData["storeId"] = id;
            TempData["storeName"] = _func.GetStoreName(id);

            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsset(dtoAsset dto)
        {
            if (ModelState.IsValid)
            {
                var asset = _db.Assets.Where(m => m.SerialNo == dto.SerialNo).FirstOrDefault();
                if (asset == null)
                {
                    var newAsset = new Asset()
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
                    TempData["storeName"] = _func.GetStoreName(dto.StoreId);
                    dto = PopulateDtoAsset(dto);

                    return View(dto);
                }
            }
            else
            {
                TempData["error"] = "Please fill all the mandatory fields!";

                TempData["storeId"] = dto.StoreId;
                TempData["storeName"] = _func.GetStoreName(dto.StoreId);

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

                    TempData["storeId"] = assetInDb.StoreId;
                    TempData["storeName"] = _func.GetStoreName(assetInDb.StoreId);

                }
            }

            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsset(dtoAsset dto)
        {
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
                    TempData["storeName"] = _func.GetStoreName(dto.StoreId);
                    return View(dto);
                }
            }
            else
            {
                dto = PopulateDtoAsset(dto);
                TempData["error"] = "All fields are mandatory!";
                TempData["storeId"] = dto.StoreId;
                TempData["storeName"] = _func.GetStoreName(dto.StoreId);
                return View(dto);
            }
        }

        [Authorize(Roles = "Administrator, StoreOwner, User")]
        [HttpGet]
        public IActionResult AssetDocuments(int id)
        {
            var dto = new dtoAssetDocument();

            dto = PopulateDtoAssetDocument(dto, id);
            int nStoreId = _func.GetStoreId(id);
            string sStoreName = _func.GetStoreName(nStoreId);

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
        public IActionResult AssetInfo(int id)
        {

            var asset = _db.Assets.Where(m => m.Id == id)
                .Include(m => m.SubCategory)
                .Include(m => m.Condition)
                .Include(m => m.SubCategory.Category)
                .Include(m => m.AssetStatus)
                .Include(m => m.Store).Include(m => m.Donor)
                .FirstOrDefault();

            if (asset != null)
            {
                TempData["storeId"] = asset.StoreId;
                TempData["storeName"] = asset.Store.Name;
            }
            else
            {
                TempData["error"] = "Record not found!";
            }
            return View(asset);
        }

        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> DeleteDocument(int id) {
            if (id > 0) {
                var assetDocument = await _db.AssetDocuments.Where(m => m.Id == id).FirstOrDefaultAsync();
                if (assetDocument != null)
                {
                    _db.AssetDocuments.Remove(assetDocument);
                    await _db.SaveChangesAsync();
                    
                    int nAssetId = assetDocument.AssetId;
                    int nStoreId = _func.GetStoreId(nAssetId);
                    

                    TempData["storeId"] = _func.GetStoreId(nAssetId);
                    TempData["storeName"] = _func.GetStoreName(nStoreId);

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
    }
}