using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using System.Data;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class StoresController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;
        private int _supervisorEmployeeId;
        public StoresController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _supervisorEmployeeId = _func.GetSupervisorId(_employeeId);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var stores = await _func.GetStoresByEmployeeIdAsync(_employeeId);

            var storeEmployees = new List<vwStoreEmployee>();

            foreach (var store in stores)
            {
                if (store != null)
                {
                    if (store.EmployeeId > 0)
                    {
                        var empl = await _db.vwEmployees
                            .FirstOrDefaultAsync(m => m.Id == store.EmployeeId);

                        if (empl != null)
                        {
                            vwStoreEmployee se = new vwStoreEmployee()
                            {
                                Id = empl.Id,
                                FullName = empl.FullName,
                                Email = empl.Email,
                                Role = empl.RoleName,
                                ImageUrl = empl.ImageUrl,
                                StoreId = store.Id
                            };
                            storeEmployees.Add(se);

                            //Add store users
                            var storeUsers = await _db.vwEmployees
                                .Where(m => m.SupervisorEmployeeId == empl.Id && m.RoleName == "User")
                                .ToListAsync();

                            foreach (var user in storeUsers)
                            {
                                vwStoreEmployee su = new vwStoreEmployee()
                                {
                                    Id = user.Id,
                                    ImageUrl = user.ImageUrl,
                                    FullName = user.FullName,
                                    Email = user.Email,
                                    Role = user.RoleName,
                                    StoreId = store.Id
                                };
                                storeEmployees.Add(su);
                            }
                        }
                    }
                }
            }

            var dto = new dtoStores()
            {
                vwStores = stores,
                storeEmployees = storeEmployees
            };

            //update dtoReportingModel for the partial view
            dto.dtoReporting.AssetStores = stores.ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            });
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
            var assetStatuses = _db.AssetStatuses.ToList().Select(m => new SelectListItem
            {
                Text = m.StatusName,
                Value = m.Id.ToString()
            });
            var assetConditions = _db.Conditions.ToList().Select(m => new SelectListItem
            {
                Text = m.ConditionName,
                Value = m.Id.ToString()
            });
            var donors = _db.Donors.ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            });

            dto.dtoReporting.AssetStatuses = assetStatuses;
            dto.dtoReporting.Categories = categories;
            dto.dtoReporting.SubCategories = subCategories;
            dto.dtoReporting.Conditions = assetConditions;
            dto.dtoReporting.Donors = donors;

            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> StoreDetails(int id)
        {
            var vwStore = await _db.vwStores.FirstOrDefaultAsync(m => m.Id == id);

            if (vwStore == null)
            {
                return RedirectToAction("Index", "Stores");
            }

            var dto = new dtoStore
            {
                vwStore = vwStore,
                employees = new List<Employee>(),
                StoreOwnerInfo = await _func.GetStoreOwnerInfoAsync(id)
            };

            var storeOwnerId = vwStore.EmployeeId;
            if (storeOwnerId > 0)
            {
                var storeOwner = await _db.Employees.FirstOrDefaultAsync(e => e.Id == storeOwnerId);
                if (storeOwner != null)
                {
                    dto.employees.Add(storeOwner);
                    dto.employees.AddRange(await _db.Employees
                        .Where(e => e.SupervisorEmployeeId == storeOwnerId)
                        .ToListAsync());
                }
            }

            dto.storeAssets = await _db.Assets
                .Where(a => a.AssetStatusId != SD.Asset_Deleted && a.StoreId == id)
                .Include(a => a.SubCategory)
                .Include(a => a.Condition)
                .Include(a => a.AssetStatus)
                .ToListAsync();

            dto.StoreCategoryAssets = await _db.vwStoreCategoryAssets
                .Where(sca => sca.StoreId == id).OrderBy(m => m.CategoryId)
                .ToListAsync();

            dto.TransferredAssets = await _db.TransferDetails
                .Include(td => td.Transfer)
                .CountAsync(td => td.Transfer.StoreFromId == id && td.Transfer.TransferStatusId == SD.Transfer_Completed);

            dto.ReceivedAssets = await _db.TransferDetails
                .Include(td => td.Transfer)
                .CountAsync(td => td.Transfer.StoreId == id && td.Transfer.TransferStatusId == SD.Transfer_Completed);

            dto.Handovers = 0;
            dto.Disposals = await _db.Assets.Where(m => m.StoreId == id && m.AssetStatusId == SD.Asset_Disposed).CountAsync();

            TempData["storeId"] = id;
            TempData["storeName"] = vwStore.Name;

            return View(dto);
        }



    }
}
