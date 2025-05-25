using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MODAMS.ApplicationServices.IServices;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using System.Data;
using System.Globalization;

namespace MODAMSWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly ICategoriesService _categoriesService;
        private bool _isSomali;

        public CategoriesController(ApplicationDbContext db, IAMSFunc func, ICategoriesService categoriesService)
        {
            _db = db;
            _func = func;
            _categoriesService = categoriesService;

            _isSomali = CultureInfo.CurrentCulture.Name == "so";
        }

        public async Task<IActionResult> Index(int? id)
        {
            var result = await _categoriesService.GetIndexAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new CategoriesDTO());
            }
        }
        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpGet]
        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null)
            {
                TempData["error"] = _isSomali ? "Fadlan dooro qayb" : "Please select a category";
                return RedirectToAction("Index", "Categories");
            }

            var categoryInDb = await _db.Categories.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (categoryInDb == null)
            {
                TempData["error"] = _isSomali? "Qayb lama helin!" : "Category not found!";
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
                TempData["error"] = _isSomali? "Dhammaan meelaha waa qasab" : "All fields are mandatory";
                return View(category);
            }

            var categoryInDb = await _db.Categories.Where(m => m.Id == category.Id).FirstOrDefaultAsync();
            if (categoryInDb == null)
            {
                TempData["error"] = _isSomali ? "Qayb lama helin!" : "Category not found!";
                return View(category);
            }

            categoryInDb.CategoryCode = category.CategoryCode;
            categoryInDb.CategoryName = category.CategoryName;

            await _db.SaveChangesAsync();
            TempData["success"] = _isSomali? "Isbedelada si guul leh ayaa loo keydiyay!" : "Changes saved successfuly!";
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
                TempData["error"] = _isSomali ? "Dhammaan meelaha waa qasab" : "All fields are mandatory";
                return View(category);
            }

            var result = await _categoriesService.CreateCategoryAsync(category);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = _isSomali? "Qayb si guul leh ayaa loo abuuray!" : "Category created successfuly!";
                return RedirectToAction("Index", "Categories");
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(category);
            }
        }
        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpGet]
        public async Task<IActionResult> CreateSubCategory(int id)
        {
            var result = await _categoriesService.GetCreateSubCategoryAsync(id);

            if (result.IsSuccess)
            {
                return View(result.Value);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Categories");
            }
        }
        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpPost]
        public async Task<IActionResult> CreateSubCategory(SubCategoryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = _isSomali ? "Dhammaan meelaha waa qasab" : "All fields are mandatory";
                return View(dto);
            }

            var result = await _categoriesService.CreateSubCategoryAsync(dto);
            dto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = _isSomali? "Qayb-hoosaad si guul leh ayaa loo daray!" : "Sub-Category added successfuly!";
                return RedirectToAction("Index", "Categories");
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(dto);
            }
        }
        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpGet]
        public async Task<IActionResult> EditSubCategory(int id)
        {
            if (id == 0)
            {
                TempData["error"] = _isSomali? "Fadlan marka hore qayb-hoosaad dooro!" : "Please select a Sub-Category to first";
                return RedirectToAction("Index", "Categories");
            }

            var result = await _categoriesService.GetEditSubCategoryAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Categories");
            }
        }
        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpPost]
        public async Task<IActionResult> EditSubCategory(SubCategoryDTO subCategory)
        {
            var result = await _categoriesService.EditSubCategoryAsync(subCategory);

            var dto = result.Value;
            if (result.IsSuccess)
            {
                TempData["success"] = _isSomali? "Isbedelada si guul leh ayaa loo keydiyay!" : "Changes saved successfuly!";
                return RedirectToAction("Index", "Categories");
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(dto);
            }
        }

        //API AJAX CALLS
        [HttpGet]
        [Authorize(Roles = "Administrator, SeniorManagement, StoreOwner")]
        public async Task<IActionResult> GetSubCategories(string categoryName)
        {
            var subCategories = await _categoriesService.GetSubCategoriesAsync(categoryName);
            return Ok(subCategories);
        }
    }
}
