using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOD_AMS.Models;
using MODAMS.DataAccess.Data;
using MODAMS.Models.ViewModels;
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

        public StoresController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
        }

        public IActionResult Index()
        {
            var stores = _db.vwStores.ToList();
            var allStores = stores;

            if (User.IsInRole("User")) {
                var nSupervisorId = _func.GetSupervisorId(_employeeId);
                stores = stores.Where(m => m.EmployeeId == nSupervisorId).ToList();
            }
            else if (User.IsInRole("StoreOwner"))
            {
                stores = stores.Where(m => m.EmployeeId == _employeeId).ToList();
                vwStores store = stores[0];
                if (store != null)
                {
                    int nDeptId = (int)store.DepartmentId;
                    var storeFinder = new StoreFinder(nDeptId, allStores);
                    stores = storeFinder.GetStores();
                }
            }
            return View(stores);
        }

        public IActionResult StoreDetails(int id)
        {
            var store = _db.Stores.Where(m => m.Id == id).FirstOrDefault();
            TempData["storeId"] = store.Id;
            TempData["storeName"] = store.Name;
            return View();
        }

    }
}
