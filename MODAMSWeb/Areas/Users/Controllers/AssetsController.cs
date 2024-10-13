using DocumentFormat.OpenXml.Office2010.Excel;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
using MODAMS.ApplicationServices;
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
    [Authorize]
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly IAssetService _assetService;

        private int _employeeId;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AssetsController(ApplicationDbContext db, IAMSFunc func, IWebHostEnvironment webHostEnvironment, IAssetService assetService)
        {
            _db = db;
            _func = func;
            _assetService = assetService;

            _employeeId = _func.GetEmployeeId();
            _webHostEnvironment = webHostEnvironment;
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
            var empId = User.IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
            if (await _func.GetStoreOwnerIdAsync(id) != empId)
            {
                TempData["error"] = "You are not authorized to perform this action!";
                return RedirectToAction("Index", "Assets", new { area = "Users", id = id });
            }

            var result = await _assetService.GetCreateAssetAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Assets", new { area = "Users", id = id });
            }
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

            TempData["success"] = "Asset registered successfully!";
            return RedirectToAction("EditAsset", "Assets", new { id = dto.Id });
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
            var empId = User.IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
            if (await _func.GetStoreOwnerIdAsync(dto.StoreId) != empId)
            {
                TempData["error"] = "You are not authorized to perform this action!";
                return RedirectToAction("Index", "Assets", new { area = "Users", id = dto.StoreId });
            }

            if (ModelState.IsValid)
            {
                var result = await _assetService.EditAssetAsync(dto);

                if (result.IsSuccess)
                {
                    TempData["success"] = "Changes saved successfully!";
                    return RedirectToAction("EditAsset", "Assets", new { id = dto.Id });
                }
                else
                {
                    TempData["error"] = result.ErrorMessage;
                    // Preserve user-entered data and repopulate SelectLists
                    dto = await _assetService.PopulateDtoAssetAsync(dto);
                    return View(dto);
                }
            }
            else
            {
                TempData["error"] = "All fields are mandatory!";
                // Repopulate only the SelectList fields, keeping the user-entered data
                dto = await _assetService.PopulateDtoAssetAsync(dto);
                return View(dto);
            }
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
            else {
                TempData["error"] = result.ErrorMessage;
                return View(new AssetDocumentDTO());
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int Id, int DocumentTypeId, IFormFile? file)
        {
            var result = await _assetService.UploadDocumentAsync(Id, DocumentTypeId, file);

            if (result.IsSuccess)
            {
                TempData["success"] = "File uploaded successfully!";
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
                TempData["storeName"] = dto.Asset.Store.Name;
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
            var assetPictures = await _db.AssetPictures.Where(m => m.AssetId == id).ToListAsync();

            var storeId = await _func.GetStoreIdByAssetIdAsync(id);
            var storeName = await _func.GetStoreNameByStoreIdAsync(storeId);

            var asset = await _db.Assets.Where(m => m.Id == id).FirstOrDefaultAsync();
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

                    //Log NewsFeed
                    string employeeName = await _func.GetEmployeeNameAsync();
                    string assetName = await _func.GetAssetNameAsync(AssetId);
                    string storeName = await _func.GetStoreNameByStoreIdAsync(await _func.GetStoreIdByAssetIdAsync(AssetId));
                    string message = $"{employeeName} uploaded a picture for ({assetName}) in {storeName}";
                    await _func.LogNewsFeedAsync(message, "Users", "Assets", "AssetInfo", AssetId);

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

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            if (id == 0)
            {
                TempData["error"] = "Asset not found!";
                return RedirectToAction("AssetInfo", "Assets", new { id = id });
            }

            var assetInDb = await _db.Assets.FirstOrDefaultAsync(m => m.Id == id);
            if (assetInDb == null)
            {
                TempData["error"] = "Asset not found!";
                return RedirectToAction("AssetInfo", "Assets", new { id = id });
            }
            else
            {
                assetInDb.AssetStatusId = 4;
                assetInDb.Remarks = $"Asset Deleted by {await _func.GetEmployeeNameAsync()}";
            }
            await _db.SaveChangesAsync();

            var assetHistory = new AssetHistory()
            {
                AssetId = id,
                Description = $"Asset Deleted by {await _func.GetEmployeeNameAsync()}",
                TimeStamp = DateTime.Now,
                TransactionRecordId = id,
                TransactionTypeId = SD.Transaction_Delete
            };
            _db.AssetHistory.Add(assetHistory);
            await _db.SaveChangesAsync();

            int storeId = await _func.GetStoreIdByAssetIdAsync(id);

            TempData["success"] = "Asset deleted successfuly!";
            return RedirectToAction("Index", "Assets", new { id = storeId });
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RecoverAsset(int id)
        {
            if (id == 0)
            {
                TempData["error"] = "Asset not found!";
                return RedirectToAction("Index", "Settings", new { area = "Admin", id = id });
            }

            var assetInDb = await _db.Assets.FirstOrDefaultAsync(m => m.Id == id);
            if (assetInDb == null)
            {
                TempData["error"] = "Asset not found!";
                return RedirectToAction("Index", "Settings", new { area = "Admin", id = id });
            }
            else
            {
                assetInDb.AssetStatusId = 1;
                assetInDb.Remarks = $"Asset Recovered by {await _func.GetEmployeeNameAsync()}";
            }
            await _db.SaveChangesAsync();

            var assetHistory = new AssetHistory()
            {
                AssetId = id,
                Description = $"Asset Recovered by {await _func.GetEmployeeNameAsync()}",
                TimeStamp = DateTime.Now,
                TransactionRecordId = id,
                TransactionTypeId = SD.Transaction_Recover
            };
            _db.AssetHistory.Add(assetHistory);
            await _db.SaveChangesAsync();


            TempData["success"] = "Asset Recovered successfuly!";
            return RedirectToAction("Index", "Settings", new { area = "Admin", id = id });
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

        //API Calls End

        //Private functions
        
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