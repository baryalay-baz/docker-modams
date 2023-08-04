using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class TransfersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;

        public TransfersController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreateTransfer()
        {
            var dto = new dtoTransfer();
            List<Asset> assets = GetAssets().GetAwaiter().GetResult();
            List<dtoTransferAsset> transferAssets = new List<dtoTransferAsset>();

            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    var transferAsset = new dtoTransferAsset()
                    {
                        AssetId = asset.Id,
                        AssetName = asset.Name,
                        Category = asset.SubCategory.Category.CategoryName,
                        SubCategory = asset.SubCategory.SubCategoryName,
                        Make = asset.Make,
                        Model = asset.Model,
                        Barcode = asset.Barcode.ToString(),
                        SerialNumber = asset.SerialNo,
                        IsSelected = false
                    };
                    transferAssets.Add(transferAsset);
                }
            }
            if(transferAssets.Count>0)
            {
                dto.Assets = transferAssets;
            }

            var storeList = _db.Stores.ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString(),
            });

            dto.StoreList = storeList;

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTransfer(dtoTransfer transferDTO) {

            var assets = transferDTO.Assets;
            var sResult = "";
            foreach (var asset in assets) {
                sResult += asset.AssetId + " - " + asset.IsSelected + "\n";
            }
 
            return RedirectToAction("CreateTransfer", "Transfers");
        }
        private async Task<List<Asset>> GetAssets()
        {
            var assets = await _db.Assets.Include(m => m.Store.Department)
                .Include(m=>m.SubCategory).Include(m=>m.SubCategory.Category)
                .Where(m=>m.AssetStatusId==1).ToListAsync();
            assets = assets.Where(m => m.Store.Department.EmployeeId == _employeeId).ToList();
            return assets;
        }
    }
}
