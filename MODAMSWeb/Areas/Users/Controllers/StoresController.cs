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
        public IActionResult Index()
        {
            var stores = _db.vwStores.OrderByDescending(m => m.TotalCount).ToList();
            var allStores = stores;

            if (User.IsInRole("User"))
            {
                stores = stores.Where(m => m.EmployeeId == _supervisorEmployeeId).ToList();
            }
            else if (User.IsInRole("StoreOwner"))
            {
                stores = stores.Where(m => m.EmployeeId == _employeeId).ToList();
                if (stores.Count > 0)
                {
                    vwStore store = stores.First();
                    if (store != null)
                    {
                        int nDeptId = (int)store.DepartmentId;
                        var storeFinder = new StoreFinder(nDeptId, allStores);
                        stores = storeFinder.GetStores();
                    }
                }
                else
                {
                    stores = new List<vwStore>();
                }
            }

            var storeEmployees = new List<vwStoreEmployee>();

            foreach (var store in stores)
            {
                if (store != null)
                {
                    if (store.EmployeeId > 0)
                    {
                        var empl = _db.vwEmployees.Where(m => m.Id == store.EmployeeId).FirstOrDefault();
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
                            var storeUsers = _db.vwEmployees.Where(m => m.SupervisorEmployeeId == empl.Id).ToList();
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
            dto.dtoReporting.Stores = stores.ToList().Select(m => new SelectListItem
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
        public IActionResult StoreDetails(int id)
        {
            var vwStore = _db.vwStores.FirstOrDefault(m => m.Id == id);

            if (vwStore == null)
            {
                return RedirectToAction("Index", "Stores");
            }

            var dto = new dtoStore
            {
                vwStore = vwStore,
                employees = new List<Employee>(),
                StoreOwnerInfo = _func.GetStoreOwnerInfo(id)
            };

            var storeOwnerId = vwStore.EmployeeId;
            if (storeOwnerId > 0)
            {
                var storeOwner = _db.Employees.FirstOrDefault(e => e.Id == storeOwnerId);
                if (storeOwner != null)
                {
                    dto.employees.Add(storeOwner);
                    dto.employees.AddRange(_db.Employees
                        .Where(e => e.SupervisorEmployeeId == storeOwnerId)
                        .ToList());
                }
            }

            dto.storeAssets = _db.Assets
                .Where(a => a.AssetStatusId != SD.Asset_Deleted && a.StoreId == id)
                .Include(a => a.SubCategory)
                .Include(a => a.Condition)
                .Include(a => a.AssetStatus)
                .ToList();

            dto.StoreCategoryAssets = _db.vwStoreCategoryAssets
                .Where(sca => sca.StoreId == id).OrderBy(m=>m.CategoryId)
                .ToList();

            dto.TransferredAssets = _db.TransferDetails
                .Include(td => td.Transfer)
                .Count(td => td.Transfer.StoreFromId == id && td.Transfer.TransferStatusId == SD.Transfer_Completed);

            dto.ReceivedAssets = _db.TransferDetails
                .Include(td => td.Transfer)
                .Count(td => td.Transfer.StoreId == id && td.Transfer.TransferStatusId == SD.Transfer_Completed);

            dto.Handovers = 0;
            dto.Disposals = _db.Assets.Where(m => m.StoreId == id && m.AssetStatusId == SD.Asset_Disposed).Count();

            TempData["storeId"] = id;
            TempData["storeName"] = vwStore.Name;

            return View(dto);
        }



    }
}
