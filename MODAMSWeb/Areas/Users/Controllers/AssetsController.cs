using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
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

        [Authorize(Roles = "Administrator, StoreOwner, User")]
        [HttpGet]
        public IActionResult CreateAsset(int id)
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

            var dto = new dtoAsset()
            {
                Categories = categories,
                SubCategories = subCategories,
                Donors = donors,
                Statuses = statuses,
                Conditions = conditions
            };

            TempData["storeId"] = id;
            TempData["storeName"] = _func.GetStoreName(id);

            return View(dto);
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsset(dtoAsset dto) {
          
            if (!ModelState.IsValid) {
                ModelState.AddModelError("error", "Form not valid!");
                return View(dto);
            }

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
                    return RedirectToAction("CreateAsset", "Assets", new { area = "Users", id = dto.StoreId });
                }
            }
            else {
                TempData["error"] = "Please fill all the mandatory fields!";
                return RedirectToAction("CreateAsset", "Assets", new { area = "Users", id = dto.StoreId });
            }
        }

        //API Calls
        [HttpGet]
        public async Task<string> GetCategories() {
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
                subCategories = subCategories.Where(m=>m.CategoryId==id).ToList();       
            
            if (subCategories != null)
            {
                sResult = JsonConvert.SerializeObject(subCategories);
            }

            return sResult;
        }
    }
}