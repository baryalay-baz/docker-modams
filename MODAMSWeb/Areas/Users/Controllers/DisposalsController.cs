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
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _employeeId = User.IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
            _storeId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);

            var dto = new dtoDisposal();

            if (_employeeId == await _func.GetStoreOwnerIdAsync(_employeeId))
                dto.IsAuthorized = true;

            var disposals = await _db.Disposals
                .Include(m => m.DisposalType).Include(m => m.Asset).Include(m => m.Asset.Store)
                .Include(m => m.Asset.SubCategory).Include(m => m.Asset.SubCategory.Category)
                .ToListAsync();

            if (User.IsInRole("User") || User.IsInRole("StoreOwner"))
            {
                disposals = disposals.Where(m => m.EmployeeId == _employeeId).ToList();
            }

            dto.Disposals = disposals;
            dto.StoreId = _storeId;
            if (await _func.GetStoreOwnerIdAsync(_storeId) == _employeeId)
            {
                dto.IsAuthorized = true;
            }
            else {
                dto.IsAuthorized = false;
            }

            var disposalChart = await _db.Disposals
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
                .ToListAsync();

            if (User.IsInRole("User") || User.IsInRole("StoreOwner"))
            {
                disposalChart = disposalChart.Where(m => m.EmployeeId == _employeeId).ToList();
            }
            dto.ChartData = disposalChart;
            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> CreateDisposal()
        {
            var dto = new dtoCreateDisposal();
            _employeeId = User.IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
            _storeId = await _func.GetStoreIdByEmployeeIdAsync(_employeeId);

            var assetList = _db.Assets.Where(m => m.AssetStatusId != SD.Asset_Deleted && m.StoreId == _storeId)
                .Include(m => m.SubCategory).Include(m => m.SubCategory.Category)
                .Where(m => m.AssetStatusId == SD.Asset_Available).ToList();

            dto.Assets = assetList;

            dto.IsAuthorized = await _func.GetStoreOwnerIdAsync(_storeId) == _employeeId ? true : false;

            dto.StoreName = await _func.GetStoreNameByStoreIdAsync(_storeId);
            dto.StoreOwner = await _func.GetEmployeeNameByIdAsync(await _func.GetStoreOwnerIdAsync(_storeId));

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
                _employeeId = await _func.GetSupervisorIdAsync(_employeeId);
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
            var asset = await _db.Assets.FirstOrDefaultAsync(m => m.Id == disposal.AssetId);
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
                Description = "Asset disposed by " + await _func.GetEmployeeNameAsync()
            };
            await _db.AssetHistory.AddAsync(assetHistory);
            await _db.SaveChangesAsync();

            //Log NewsFeed
            string employeeName = await _func.GetEmployeeNameAsync();
            string assetName = await _func.GetAssetNameAsync(disposal.AssetId);
            string storeName = await _func.GetStoreNameByStoreIdAsync(await _func.GetStoreIdByAssetIdAsync(disposal.AssetId));
            string message = $"{employeeName} dipsosed an asset ({assetName}) in {storeName}";
            await _func.LogNewsFeedAsync(message, "Users", "Disposals", "EditDiposal", disposal.AssetId);

            TempData["success"] = "Disposal added successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> EditDisposal(int id)
        {
            var employeeId = _employeeId;

            if (User.IsInRole("User"))
            {
                employeeId = await _func.GetSupervisorIdAsync(_employeeId);
            }

            var storeId = await _func.GetStoreIdByEmployeeIdAsync(employeeId);

            var disposal = await _db.Disposals
                .Where(m => m.Id == id)
                .Include(m => m.DisposalType)
                .Include(m => m.Asset)
                    .ThenInclude(a => a.Store)
                .Include(m => m.Asset.SubCategory)
                    .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync();

            if (disposal == null)
            {
                TempData["error"] = "Disposal not found!";
                return RedirectToAction("Index", "Disposals");
            }

            var dto = new dtoEditDisposal
            {
                Disposal = disposal,
                Assets = await _db.Assets.Where(a => a.StoreId == storeId && a.AssetStatusId != SD.Asset_Deleted && a.AssetStatusId != SD.Asset_Disposed)
                    .Include(a => a.SubCategory).ThenInclude(s => s.Category).ToListAsync(),

                IsAuthorized = await _func.GetStoreOwnerIdAsync(storeId) == employeeId,
                StoreName = await _func.GetStoreNameByStoreIdAsync(storeId),
                StoreOwner = await _func.GetEmployeeNameByIdAsync(await _func.GetStoreOwnerIdAsync(storeId)),
                DisposalTypeList = await _db.DisposalTypes
                    .Select(dt => new SelectListItem
                    {
                        Text = dt.Type,
                        Value = dt.Id.ToString()
                    }).ToListAsync()
            };
            var currentDisposedAsset = await _db.Assets
                .Include(m => m.SubCategory).ThenInclude(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == disposal.AssetId);

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
                _employeeId = await _func.GetSupervisorIdAsync(_employeeId);
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
                    Description = "Asset un-disposed by " + await _func.GetEmployeeNameAsync()
                };
                _db.AssetHistory.Add(assetHistory);

                //Add history for the new asset
                var newAssetHistory = new AssetHistory()
                {
                    TimeStamp = DateTime.Now,
                    AssetId = disposal.AssetId,
                    TransactionRecordId = disposal.AssetId,
                    TransactionTypeId = SD.Transaction_Disposal,
                    Description = "Asset disposed by " + await _func.GetEmployeeNameAsync()
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
            string employeeName = await _func.GetEmployeeNameAsync();
            string assetName = await _func.GetAssetNameAsync(disposal.AssetId);
            string storeName = await _func.GetStoreNameByStoreIdAsync(await _func.GetStoreIdByAssetIdAsync(disposal.AssetId));
            string message = $"{employeeName} modified disposal record for an asset ({assetName}) in {storeName}";
            await _func.LogNewsFeedAsync(message, "Users", "Disposals", "EditDisposal", disposal.Id);

            TempData["success"] = "Disposal updated successfully!";
            return RedirectToAction("EditDisposal", "Disposals", new { disposal.Id });
        }

    }
}
