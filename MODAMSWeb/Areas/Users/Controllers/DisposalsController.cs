using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using Org.BouncyCastle.Ocsp;

namespace MODAMSWeb.Areas.Users.Controllers
{

    [Area("Users")]
    [Authorize]
    public class DisposalsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;
        private int _storeId;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DisposalsController(ApplicationDbContext db, IAMSFunc func, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _webHostEnvironment = webHostEnvironment;

        }

        public IActionResult Index()
        {
            _employeeId = User.IsInRole("User") ? _func.GetSupervisorId(_employeeId) : _employeeId;
            _storeId = _func.GetStoreIdByEmployeeId(_employeeId);



            var dto = new dtoDisposal();

            if (_employeeId == _func.GetStoreOwnerId(_employeeId))
                dto.IsAuthorized = true;

            var disposals = _db.Disposals
                .Include(m => m.DisposalType).Include(m => m.Asset).Include(m => m.Asset.Store)
                .Include(m=>m.Asset.SubCategory).Include(m=>m.Asset.SubCategory.Category)
                .ToList();

            if (User.IsInRole("User") || User.IsInRole("StoreOwner")) {
                disposals = disposals.Where(m => m.EmployeeId == _employeeId).ToList();
            }

            dto.Disposals = disposals;
            dto.StoreId = _storeId;
            dto.IsAuthorized = true;

            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = "StoreOwner, User")]
        public IActionResult CreateDisposal() {
            var dto = new dtoCreateDisposal();
            _employeeId = User.IsInRole("User") ? _func.GetSupervisorId(_employeeId) : _employeeId;
            _storeId = _func.GetStoreIdByEmployeeId(_employeeId);

            var assetList = _db.Assets.Where(m => m.StoreId == _storeId)
                .Include(m=>m.SubCategory).Include(m=>m.SubCategory.Category)
                .Where(m=>m.AssetStatusId==SD.Asset_Available).ToList();
            
            dto.Assets = assetList;
            dto.IsAuthorized = true;

            dto.StoreName = _func.GetStoreNameByStoreId(_storeId);
            dto.StoreOwner = _func.GetEmployeeNameById(_func.GetStoreOwnerId(_storeId));

            var disposalTypeList = _db.DisposalTypes.ToList().Select(m => new SelectListItem
            {
                Text = m.Type,
                Value = m.Id.ToString()
            });
            dto.DisposalTypeList = disposalTypeList;

            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "StoreOwner, User")]
        public IActionResult CreateDisposal(dtoCreateDisposal dto)
        {

            // Get the supervisor ID if the user is in the "User" role
            if (User.IsInRole("User"))
            {
                _employeeId = _func.GetSupervisorId(_employeeId);
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill all the mandatory fields!";
                return RedirectToAction("CreateDisposal", "Disposals");
            }

            string sFileName = "";

            if (dto.file != null)
            {
                // Construct the file path
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                var fileNameGuid = Guid.NewGuid().ToString();
                var targetDir = "disposaldocuments"; // Change this to your desired directory
                var fileExtension = Path.GetExtension(dto.file.FileName);
                var uniqueFileName = fileNameGuid + fileExtension;
                var filePath = Path.Combine(wwwRootPath, targetDir, uniqueFileName);

                // Save the uploaded file to the directory
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    dto.file.CopyTo(stream);
                }

                sFileName = uniqueFileName;
            }

            var disposal = dto.Disposal;
            disposal.ImageUrl = sFileName;
            disposal.EmployeeId = _employeeId;

            // Add and save the disposal entity
            _db.Disposals.Add(disposal);
            _db.SaveChanges();

            // Update asset status if an asset is associated with the disposal
            var asset = _db.Assets.FirstOrDefault(m => m.Id == disposal.AssetId);
            if (asset != null)
            {
                asset.AssetStatusId = SD.Asset_Disposed;
                _db.SaveChanges(); // Save changes to the asset status
            }

            var assetHistory = new AssetHistory()
            {
                TimeStamp = DateTime.Now,
                AssetId = disposal.AssetId,
                TransactionRecordId = disposal.AssetId,
                TransactionTypeId = SD.Transaction_Disposal,
                Description = "Asset disposed by " + _func.GetEmployeeName(_employeeId)
            };
            _db.AssetHistory.Add(assetHistory);
            _db.SaveChanges();

            TempData["success"] = "Disposal added successfully!";
            return RedirectToAction("Index");
        }

    }
}
