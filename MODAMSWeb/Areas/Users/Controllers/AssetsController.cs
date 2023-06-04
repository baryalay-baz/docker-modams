using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Utility;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;


namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;

        public AssetsController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
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

            dto = PopulateDtoAssetDocument(dto);
            int nStoreId = _func.GetStoreId(id);
            string sStoreName = _func.GetStoreName(nStoreId);

            TempData["storeId"] = nStoreId;
            TempData["storeName"] = sStoreName;

            return View(dto);
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
        private dtoAssetDocument PopulateDtoAssetDocument(dtoAssetDocument dto)
        {

            var documentTypes = _db.DocumentTypes.ToList();
            dto.DocumentTypes = documentTypes;

            return dto;
        }
    }
}