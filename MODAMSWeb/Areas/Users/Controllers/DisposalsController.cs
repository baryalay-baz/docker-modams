using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using NuGet.ContentModel;
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
                .Include(m => m.Asset.SubCategory).Include(m => m.Asset.SubCategory.Category)
                .ToList();

            if (User.IsInRole("User") || User.IsInRole("StoreOwner"))
            {
                disposals = disposals.Where(m => m.EmployeeId == _employeeId).ToList();
            }

            dto.Disposals = disposals;
            dto.StoreId = _storeId;
            if (_func.GetStoreOwnerId(_storeId) == _employeeId)
            {
                dto.IsAuthorized = true;
            }
            else {
                dto.IsAuthorized = false;
            }

            var disposalChart = _db.Disposals
                .Join(
                    _db.DisposalTypes,
                    disposal => disposal.DisposalTypeId,
                    disposalType => disposalType.Id,
                    (disposal, disposalType) => new { Disposal = disposal, DisposalType = disposalType }
                )
                .GroupBy(
                    joined => new { joined.DisposalType.Type, joined.Disposal.EmployeeId },
                    (key, grouped) => new DisposalChart
                    {
                        Type = key.Type,
                        EmployeeId = key.EmployeeId,
                        Count = grouped.Count()
                    }
                )
                .ToList();

            if (User.IsInRole("User") || User.IsInRole("StoreOwner"))
            {
                disposalChart = disposalChart.Where(m => m.EmployeeId == _employeeId).ToList();
            }
            dto.ChartData = disposalChart;
            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = "StoreOwner, User")]
        public IActionResult CreateDisposal()
        {
            var dto = new dtoCreateDisposal();
            _employeeId = User.IsInRole("User") ? _func.GetSupervisorId(_employeeId) : _employeeId;
            _storeId = _func.GetStoreIdByEmployeeId(_employeeId);

            var assetList = _db.Assets.Where(m => m.AssetStatusId != SD.Asset_Deleted && m.StoreId == _storeId)
                .Include(m => m.SubCategory).Include(m => m.SubCategory.Category)
                .Where(m => m.AssetStatusId == SD.Asset_Available).ToList();

            dto.Assets = assetList;

            dto.IsAuthorized = _func.GetStoreOwnerId(_storeId) == _employeeId ? true : false;

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

        //Post CreateDisposal form
        [HttpPost]
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> CreateDisposal(dtoCreateDisposal dto)
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
            await _db.SaveChangesAsync();

            // Update asset status if an asset is associated with the disposal
            var asset = _db.Assets.FirstOrDefault(m => m.Id == disposal.AssetId);
            if (asset != null)
            {
                asset.AssetStatusId = SD.Asset_Disposed;
                await _db.SaveChangesAsync(); // Save changes to the asset status
            }

            var assetHistory = new AssetHistory()
            {
                TimeStamp = DateTime.Now,
                AssetId = disposal.AssetId,
                TransactionRecordId = disposal.AssetId,
                TransactionTypeId = SD.Transaction_Disposal,
                Description = "Asset disposed by " + await _func.GetEmployeeName(_employeeId)
            };
            _db.AssetHistory.Add(assetHistory);
            await _db.SaveChangesAsync();

            //Log NewsFeed
            string employeeName = await _func.GetEmployeeName();
            string assetName = _func.GetAssetName(disposal.AssetId);
            string storeName = _func.GetStoreNameByStoreId(_func.GetStoreIdByAssetId(disposal.AssetId));
            string message = $"{employeeName} dipsosed an asset ({assetName}) in {storeName}";
            _func.LogNewsFeed(message, "Users", "Disposals", "EditDiposal", disposal.AssetId);

            TempData["success"] = "Disposal added successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "StoreOwner, User")]
        public IActionResult EditDisposal(int id)
        {
            var employeeId = _employeeId;

            if (User.IsInRole("User"))
            {
                employeeId = _func.GetSupervisorId(employeeId);
            }

            var storeId = _func.GetStoreIdByEmployeeId(employeeId);

            var disposal = _db.Disposals
                .Where(m => m.Id == id)
                .Include(m => m.DisposalType)
                .Include(m => m.Asset)
                    .ThenInclude(a => a.Store)
                .Include(m => m.Asset.SubCategory)
                    .ThenInclude(s => s.Category)
                .FirstOrDefault();

            if (disposal == null)
            {
                TempData["error"] = "Disposal not found!";
                return RedirectToAction("Index", "Disposals");
            }

            var dto = new dtoEditDisposal
            {
                Disposal = disposal,
                Assets = _db.Assets.Where(a => a.StoreId == storeId && a.AssetStatusId != SD.Asset_Deleted && a.AssetStatusId != SD.Asset_Disposed)
                    .Include(a => a.SubCategory).ThenInclude(s => s.Category).ToList(),

                IsAuthorized = _func.GetStoreOwnerId(storeId) == employeeId,
                StoreName = _func.GetStoreNameByStoreId(storeId),
                StoreOwner = _func.GetEmployeeNameById(_func.GetStoreOwnerId(storeId)),
                DisposalTypeList = _db.DisposalTypes
                    .ToList()
                    .Select(dt => new SelectListItem
                    {
                        Text = dt.Type,
                        Value = dt.Id.ToString()
                    }),
            };
            var currentDisposedAsset = _db.Assets
                .Include(m => m.SubCategory).ThenInclude(m => m.Category)
                .FirstOrDefault(m => m.Id == disposal.AssetId);

            if (currentDisposedAsset != null)
            {
                dto.CurrentDisposedAsset = currentDisposedAsset;
            }


            return View(dto);
        }


        [HttpPost]
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> EditDisposal(dtoEditDisposal dto)
        {

            // Get the supervisor ID if the user is in the "User" role
            if (User.IsInRole("User"))
            {
                _employeeId = _func.GetSupervisorId(_employeeId);
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill all the mandatory fields!";
                return RedirectToAction("EditDisposal", "Disposals", new { dto.Disposal.Id });
            }

            string sFileName = "";

            if (dto.file != null)
            {
                // Construct the file path
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                var fileNameGuid = Guid.NewGuid().ToString();
                var targetDir = "disposaldocuments";
                var fileExtension = Path.GetExtension(dto.file.FileName);
                var uniqueFileName = fileNameGuid + fileExtension;
                var filePath = Path.Combine(wwwRootPath, targetDir, uniqueFileName);

                // Save the uploaded file to the directory
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.file.CopyToAsync(stream);
                }

                sFileName = uniqueFileName;
            }

            var disposal = dto.Disposal;
            if (dto.file != null)
                disposal.ImageUrl = sFileName;

            disposal.EmployeeId = _employeeId;

            //Update the existing disposal record
            var disposalInDb = _db.Disposals.FirstOrDefault(d => d.Id == dto.Disposal.Id);

            if (disposalInDb == null)
            {
                TempData["error"] = "Record not found!";
                return RedirectToAction("EditDisposal", "Disposals", new { dto.Disposal.Id });
            }

            int prevAssetId = disposalInDb.AssetId;

            _db.Entry(disposalInDb).CurrentValues.SetValues(disposal);
            await _db.SaveChangesAsync();



            //Check if the AssetId has been changed in the update
            if (disposal.AssetId != prevAssetId)
            {
                //Asset has been changed, first un-dispose the previous asset
                var assetInDb = _db.Assets.FirstOrDefault(m => m.Id == prevAssetId);
                if (assetInDb == null)
                {
                    TempData["error"] = "Asset not found!";
                    return RedirectToAction("EditDisposal", "Disposals", new { dto.Disposal.Id });
                }
                assetInDb.AssetStatusId = SD.Asset_Available;
                _db.SaveChanges();

                //Add a new history the previous asset
                var assetHistory = new AssetHistory()
                {
                    TimeStamp = DateTime.Now,
                    AssetId = prevAssetId,
                    TransactionRecordId = prevAssetId,
                    TransactionTypeId = SD.Transaction_Disposal,
                    Description = "Asset un-disposed by " + await _func.GetEmployeeName(_employeeId)
                };
                _db.AssetHistory.Add(assetHistory);

                //Add history for the new asset
                var newAssetHistory = new AssetHistory()
                {
                    TimeStamp = DateTime.Now,
                    AssetId = disposal.AssetId,
                    TransactionRecordId = disposal.AssetId,
                    TransactionTypeId = SD.Transaction_Disposal,
                    Description = "Asset disposed by " + await _func.GetEmployeeName(_employeeId)
                };
                _db.AssetHistory.Add(newAssetHistory);
                _db.SaveChanges();
            }

            // Update asset status if an asset is associated with the disposal
            var asset = _db.Assets.FirstOrDefault(m => m.Id == disposal.AssetId);
            if (asset != null)
            {
                asset.AssetStatusId = SD.Asset_Disposed;
                await _db.SaveChangesAsync(); // Save changes to the asset status
            }

            //Log NewsFeed
            string employeeName = await _func.GetEmployeeName();
            string assetName = _func.GetAssetName(disposal.AssetId);
            string storeName = _func.GetStoreNameByStoreId(_func.GetStoreIdByAssetId(disposal.AssetId));
            string message = $"{employeeName} modified disposal record for an asset ({assetName}) in {storeName}";
            _func.LogNewsFeed(message, "Users", "Disposals", "EditDisposal", disposal.Id);

            TempData["success"] = "Disposal updated successfully!";
            return RedirectToAction("EditDisposal", "Disposals", new { disposal.Id });
        }

    }
}
