using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Utility;
using Newtonsoft.Json;
using System.Data;

namespace MODAMSWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator, SeniorManagement, StoreOwner")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;

        public CategoriesController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
        }

        public IActionResult Index(int? id)
        {
            var categories = _db.Categories.ToList();
            List<SubCategory> subcategories = new List<SubCategory>();
            var dto = new dtoCategories();

            dto.categories = categories;

            if (id != null)
            {
                subcategories = _db.SubCategories.Where(m => m.CategoryId == id).ToList();
                dto.subCategories = subcategories;
                dto.SelectedCategoryId = id;
            }
            else
            {
                dto.subCategories = subcategories;
            }

            return View(dto);
        }

        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpGet]
        public async Task<IActionResult> EditCategory(int? id)
        {

            if (id == null)
            {
                TempData["error"] = "Please select a category";
                return RedirectToAction("Index", "Categories");
            }

            var categoryInDb = await _db.Categories.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (categoryInDb == null)
            {
                TempData["error"] = "Category not found!";
                return RedirectToAction("Index", "Categories");
            }

            return View(categoryInDb);
        }

        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpPost]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "All fields are mandatory";
                return View(category);
            }

            var categoryInDb = await _db.Categories.Where(m => m.Id == category.Id).FirstOrDefaultAsync();
            if (categoryInDb == null)
            {
                TempData["error"] = "Category not found!";
                return View(category);
            }

            categoryInDb.CategoryCode = category.CategoryCode;
            categoryInDb.CategoryName = category.CategoryName;

            await _db.SaveChangesAsync();
            TempData["success"] = "Changes saved successfuly!";
            return RedirectToAction("Index", "Categories");
        }

        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpGet]
        public IActionResult CreateCategory()
        {
            var category = new Category();
            return View(category);
        }

        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "All fields are mandatory";
                return View(category);
            }

            var categoryInDb = await _db.Categories.Where(m => m.CategoryName == category.CategoryName).FirstOrDefaultAsync();
            if (categoryInDb != null)
            {
                TempData["error"] = "Category with this name already exists!";
                return View(category);
            }
            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();

            TempData["success"] = "Category added successfuly!";
            return RedirectToAction("Index", "Categories");
        }

        //Sub-Categories section
        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpGet]
        public IActionResult CreateSubCategory(int id)
        {
            var subCategory = new SubCategory();

            if (id > 0)
            {
                subCategory.CategoryId = id;
            }
            else
            {
                TempData["error"] = "Please select a category first!";
                return RedirectToAction("Index", "Categories");
            }
            return View(subCategory);
        }

        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpPost]
        public async Task<IActionResult> CreateSubCategory(SubCategory subCategory)
        {
            if(subCategory != null)
            {
                var subCategoryInDb = await _db.SubCategories.Where(m=>m.SubCategoryName==subCategory.SubCategoryName).FirstOrDefaultAsync();
                if (subCategoryInDb != null)
                {
                    TempData["error"] = "Sub-Category with this name already exists!";
                    return View(subCategory);
                }

                await _db.SubCategories.AddAsync(subCategory);
                await _db.SaveChangesAsync();

                TempData["success"] = "Sub-Category added successfuly!";
                return RedirectToAction("Index", "Categories");
            }

            return View(subCategory);
        }


        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpGet]
        public IActionResult EditSubCategory(int id)
        {
            if (id == 0)
            {
                TempData["error"] = "Please select a subcategory to first";
                return RedirectToAction("Index", "Categories");
            }
            var subCategory = _db.SubCategories.Where(m=>m.Id==id).FirstOrDefault();

            if(subCategory == null)
            {
                TempData["error"] = "Sub-Category not found!";
                return RedirectToAction("Index", "Categories");
            }

            return View(subCategory);
        }

        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpPost]
        public async Task<IActionResult> EditSubCategory(SubCategory subCategory)
        {
            if (subCategory != null)
            {
                var subCategoryInDb = await _db.SubCategories.Where(m => m.Id==subCategory.Id).FirstOrDefaultAsync();
                if (subCategoryInDb == null)
                {
                    TempData["error"] = "Sub-Category not found!";
                    return View(subCategory);
                }

                subCategoryInDb.SubCategoryCode = subCategory.SubCategoryCode;
                subCategoryInDb.SubCategoryName = subCategory.SubCategoryName;
                subCategoryInDb.LifeSpan = subCategory.LifeSpan;
                await _db.SaveChangesAsync();

                TempData["success"] = "Changes saved successfuly!";
                return RedirectToAction("Index", "Categories");
            }

            return View(subCategory);
        }


        //API AJAX CALLS
        [HttpGet]
        [Authorize(Roles = "Administrator, SeniorManagement, StoreOwner")]
        public async Task<string> GetSubCategories(string CategoryName)
        {
            string sResult = "No Records Found";

            if (CategoryName != null)
            {
                if (CategoryName == "All")
                {
                    var subCategories = _db.SubCategories.ToList();
                    if (subCategories.Count > 0)
                    {
                        sResult = JsonConvert.SerializeObject(subCategories);
                    }
                }
                else
                {
                    int nCategoryId = await _db.Categories.Where(m => m.CategoryName == CategoryName)
                        .Select(m => m.Id).FirstOrDefaultAsync();
                    if (nCategoryId > 0)
                    {
                        var subCategories = _db.SubCategories.Where(m => m.CategoryId == nCategoryId).ToList();
                        if (subCategories.Count > 0)
                        {
                            sResult = JsonConvert.SerializeObject(subCategories);
                        }
                    }
                }
            }
            return sResult;
        }
    }
}
