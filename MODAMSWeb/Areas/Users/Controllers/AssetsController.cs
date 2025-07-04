﻿
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MODAMS.ApplicationServices.IServices;
using MODAMS.DataAccess.Data;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using System.Globalization;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class AssetsController : Controller
    {
        private readonly IAMSFunc _func;
        private readonly IAssetService _assetService;

        private int _employeeId;
        private bool _isSomali;
        public AssetsController(ApplicationDbContext db, IAMSFunc func, IAssetService assetService)
        {
            _func = func;
            _assetService = assetService;

            _employeeId = _func.GetEmployeeId();
            _isSomali = CultureInfo.CurrentUICulture.Name == "so";
        }
        [HttpGet]
        public async Task<IActionResult> Index(int id, int subCategoryId = 0)
        {
            var result = await _assetService.GetIndexAsync(id, subCategoryId);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new AssetsDTO());
            }
        }
        [HttpGet]
        public async Task<IActionResult> AssetList(int id)
        {
            var result = await _assetService.GetAssetListAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new AssetListDTO());
            }
        }
        [Authorize(Roles = "StoreOwner, User")]
        [HttpGet]
        public async Task<IActionResult> CreateAsset(int id)
        {
            if (!await _func.CanModifyStoreAsync(id, _employeeId))
            {
                var message = _isSomali
                    ? "Uma aad fasaxin inaad fuliso tallaabadan!"
                    : "You are not authorized to perform this action!";
                TempData["error"] = message;
                return RedirectToAction(nameof(Index), "Assets", new { area = "Users", id = id });
            }

            var result = await _assetService.GetCreateAssetAsync(id);
            if (!result.IsSuccess)
            {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index), "Assets", new { area = "Users", id = id });
            }
            return View(result.Value);
        }
        [Authorize(Roles = "StoreOwner, User")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> CreateAsset(AssetCreateDTO dto)
        {
            var result = await _assetService.CreateAssetAsync(dto);

            if (!result.IsSuccess)
            {
                TempData["error"] = result.ErrorMessage;
                TempData["storeId"] = dto.StoreId;
                TempData["storeName"] = await _func.GetStoreNameByStoreIdAsync(dto.StoreId);

                // If the service returns the DTO (in case of validation errors), repopulate the form
                dto = result.Value;
                return View(dto);
            }

            TempData["success"] = _isSomali ? "Hantida si guul leh ayaa loo diiwaangeliyey" : "Asset registered successfully!";
            return RedirectToAction("EditAsset", "Assets", new { id = result.Value.Id });
        }
        [Authorize(Roles = "StoreOwner, User")]
        [HttpGet]
        public async Task<IActionResult> EditAsset(int id)
        {
            var result = await _assetService.GetEditAssetAsync(id);
            var dto = result.Value;

            if (!result.IsSuccess)
            {
                TempData["error"] = result.ErrorMessage;
                return View(dto);
            }

            return View(dto);
        }
        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsset(AssetEditDTO dto)
        {
            if (!await _func.CanModifyStoreAsync(dto.StoreId, _employeeId))
            {
                var message = _isSomali
                    ? "Uma aad fasaxin inaad fuliso tallaabadan!"
                    : "You are not authorized to perform this action!";
                TempData["error"] = message;
                return RedirectToAction(nameof(EditAsset), new { id = dto.StoreId });
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = _isSomali
                    ? "Dhammaan meelaha waa qasab in la buuxiyo!"
                    : "All fields are mandatory!";
                dto = await _assetService.PopulateDtoListsAsync(dto);
                return View(dto);
            }

            var result = await _assetService.EditAssetAsync(dto);
            if (!result.IsSuccess)
            {
                TempData["error"] = result.ErrorMessage;
                await _assetService.PopulateDtoListsAsync(result.Value);
                return View(result.Value);
            }

            TempData["success"] = _isSomali
                ? "Isbeddelada si guul leh ayaa loo kaydiyey!"
                : "Changes saved successfully!";
            return RedirectToAction(nameof(EditAsset), new { id = result.Value.Id });
        }
        [Authorize(Roles = "Administrator, StoreOwner, User")]
        [HttpGet]
        public async Task<IActionResult> AssetDocuments(int id)
        {
            var result = await _assetService.GetAssetDocumentsAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Assets", new { area = "Users", id = dto.StoreId });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int Id, int DocumentTypeId, IFormFile? file)
        {
            var result = await _assetService.UploadDocumentAsync(Id, DocumentTypeId, file);

            if (result.IsSuccess)
            {
                TempData["success"] = _isSomali ? "Faylka si guul leh ayaa loo soo geliyey!" : "File uploaded successfully!";
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
            }

            return RedirectToAction("AssetDocuments", "Assets", new { area = "Users", id = Id });
        }
        [HttpGet]
        public async Task<IActionResult> AssetInfo(int id, int page = 1, int tab = 1, int categoryId = 0)
        {
            var result = await _assetService.GetAssetInfoAsync(id, page, tab, categoryId);

            if (result.IsSuccess)
            {
                var dto = result.Value;
                TempData["storeId"] = dto.Asset.StoreId;
                TempData["storeName"] = _isSomali ? dto.Asset.Store.NameSo : dto.Asset.Store.Name;
                TempData["categoryId"] = categoryId;
                TempData["tab"] = tab.ToString();
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Assets");
            }
        }
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var result = await _assetService.DeleteDocumentAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return RedirectToAction("AssetDocuments", "Assets", new { id = dto.AssetId });
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View();
            }
        }
        [HttpGet]
        public async Task<IActionResult> AssetPictures(int id)
        {
            var result = await _assetService.GetAssetPicturesAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(dto);
            }
        }
        [HttpPost]
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> UploadPicture(int AssetId, IFormFile? file)
        {
            var result = await _assetService.UploadPictureAsync(AssetId, file);

            if (result.IsSuccess)
            {
                TempData["success"] = _isSomali ? "Sawirka si guul leh ayaa loo soo geliyey!" : "Picture uploaded successfully!";
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
            }

            return RedirectToAction("AssetPictures", "Assets", new { area = "Users", id = AssetId });
        }
        [HttpPost]
        [Authorize(Roles = "StoreOwner, User")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeletePicture(int id, int assetId)
        {
            var result = await _assetService.DeletePictureAsync(id);

            if (result.IsSuccess)
            {
                TempData["success"] = _isSomali ? "Sawirka si guul leh ayaa loo tirtiray!" : "Picture deleted successfuly!";
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
            }

            return RedirectToAction("AssetPictures", "Assets", new { area = "Users", id = assetId });
        }
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            var result = await _assetService.DeleteAssetAsync(id);
            int storeId = await _func.GetStoreIdByAssetIdAsync(id);

            if (result.IsSuccess)
            {
                TempData["success"] = result.ErrorMessage;
                return RedirectToAction("Index", "Assets", new { id = storeId });
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("AssetInfo", "Assets", new { id });
            }
        }
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RecoverAsset(int id)
        {
            var result = await _assetService.RecoverAssetAsync(id);

            if (result.IsSuccess)
            {
                TempData["success"] = _isSomali ? "Hantida si guul leh ayaa loo soo celiyey!" : "Asset recovered successfuly!";
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
            }

            return RedirectToAction("Index", "Settings", new { area = "Admin", id });
        }

        //API Calls
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _assetService.GetCategoriesAsync();

            if (result.IsSuccess)
            {
                return Json(new { success = true, data = result.Value });
            }
            else
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int? id)
        {
            var result = await _assetService.GetSubCategoriesAsync(id);

            if (result.IsSuccess)
            {
                return Json(new { success = true, data = result.Value });
            }
            else
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetDocumentTypes()
        {
            var result = await _assetService.GetDocumentTypesAsync();

            if (result.IsSuccess)
            {
                return Json(new { success = true, data = result.Value });
            }
            else
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetLastAssetId()
        {
            var result = await _assetService.GetLastAssetIdAsync();
            if (result.IsSuccess)
            {
                return Json(result.Value);
            }
            else
            {
                TempData["error"] = _isSomali ? "Khalad ayaa dhacay marki la helayay Aqoonsiga Hantida ugu dambeysay" : "Error occurred getting last Asset Id";
                return Json(0);
            }
        }
    }
}