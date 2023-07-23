using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOD_AMS.Models;
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

        public IActionResult Index()
        {
            var stores = _db.vwStores.OrderByDescending(m => m.TotalCost).ToList();
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
                        var empl = _db.Employees.Where(m => m.Id == store.EmployeeId).FirstOrDefault();
                        if (empl != null)
                        {
                            vwStoreEmployee se = new vwStoreEmployee()
                            {
                                Id = empl.Id,
                                ImageUrl = empl.ImageUrl,
                                StoreId = store.Id
                            };
                            storeEmployees.Add(se);

                            //Add store users
                            var storeUsers = _db.Employees.Where(m => m.SupervisorEmployeeId == empl.Id).ToList();
                            foreach (var user in storeUsers)
                            {
                                vwStoreEmployee su = new vwStoreEmployee()
                                {
                                    Id = user.Id,
                                    ImageUrl = user.ImageUrl,
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

            return View(dto);
        }

        public IActionResult StoreDetails(int id)
        {
            var vwStore = _db.vwStores.Where(m => m.Id == id).FirstOrDefault();

            var dto = new dtoStore();

            if (vwStore != null)
            {
                dto.vwStore = vwStore;

                var storeOwnerId = vwStore.EmployeeId;
                if (storeOwnerId > 0)
                {
                    var employees = _db.Employees.ToList();
                    var employee = employees.Where(m => m.Id == storeOwnerId).FirstOrDefault();
                    if (employee != null)
                    {
                        dto.employees.Add(employee);

                        var employee_users = employees.Where(m => m.SupervisorEmployeeId == employee.Id).ToList();
                        if (employee_users != null)
                        {
                            dto.employees.AddRange(employee_users);
                        }
                    }
                }
                var storeCategoryAssets = _db.vwStoreCategoryAssets
                    .Where(m => m.StoreId == id).ToList();

                var assets = _db.Assets.Where(m => m.StoreId == id)
                    .Include(m => m.SubCategory).Include(m => m.Condition).Include(m => m.AssetStatus)
                    .ToList();

                dto.storeAssets = assets;
                dto.StoreCategoryAssets = storeCategoryAssets;

                TempData["storeId"] = id;
                TempData["storeName"] = vwStore.Name;
            }
            return View(dto);
        }

    }
}
