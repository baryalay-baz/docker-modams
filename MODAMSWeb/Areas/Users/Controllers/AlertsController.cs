using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Models.ViewModels;
using MODAMS.DataAccess.Data;
using MODAMS.Utility;
using NuGet.ContentModel;
using MODAMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using MOD_AMS.Models;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class AlertsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;

        public AlertsController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
        }

        public async Task<IActionResult> Index(int? departmentId = 0)
        {
            List<vwAlert> Alerts = await GetAlerts();
            var dto = new dtoAlerts();

            var departments = Alerts.Select(m => new { m.DepartmentId, m.Department }).Distinct();

            if (departmentId > 0) {
                Alerts = Alerts.Where(m=>m.DepartmentId == departmentId).ToList();
            }
            dto.Alerts = Alerts;
            
            var departmentList = departments.Select(m => new SelectListItem
            {
                Text = m.Department,
                Value = m.DepartmentId.ToString(),
                Selected = (m.DepartmentId == departmentId)
            });

            dto.DepartmentList = departmentList;

            return View(dto);
        }
        public string GetAlertCount() {
            List<vwAlert> Alerts = GetAlerts().GetAwaiter().GetResult();
            return Alerts.Count.ToString();
        }

        //Alerts Retrieval
        private async Task<List<vwAlert>> GetAlerts()
        {
            var assets = await _db.Assets.Include(m => m.SubCategory)
                .Include(m => m.Store.Department)
                .Select(m => new
                {
                    m.Id,
                    m.SubCategoryId,
                    m.Make,
                    m.Model,
                    m.Name,
                    m.SubCategory,
                    m.Store,
                    m.Store.Department
                }).ToListAsync();

            var assetDocs = await _db.AssetDocuments.ToListAsync();
            var documentTypes = await _db.DocumentTypes.ToListAsync();

            List<vwAlert> Alerts = new List<vwAlert>();

            foreach (var asset in assets)
            {
                var blnCheck = false;
                foreach (var documentType in documentTypes)
                {
                    var docs = assetDocs
                        .Where(m => m.DocumentTypeId == documentType.Id && m.AssetId == asset.Id)
                        .ToList();

                    if (!docs.Any())
                    {
                        blnCheck = true; break;
                    }
                }
                if (blnCheck)
                {
                    var alert = new vwAlert()
                    {
                        AssetId = asset.Id,
                        SubCategory = asset.SubCategory.SubCategoryName,
                        Make = asset.Make,
                        DepartmentId = asset.Store.Department.Id,
                        Department = asset.Store.Department.Name,
                        Name = asset.Name,
                        AlertType = "Missing Documents",
                        EmployeeId = asset.Store.Department.EmployeeId
                    };
                    Alerts.Add(alert);
                }
            }

            var alertList = new List<vwAlert>();

            if (User.IsInRole("User"))
            {
                _employeeId = _func.GetSupervisorId(_employeeId);
            }


            if (User.IsInRole("StoreOwner") || User.IsInRole("User"))
            {
                var allStores = _db.vwStores.ToList();
                int DepartmentId = _func.GetDepartmentId(_employeeId);
                var storeFinder = new StoreFinder(DepartmentId, allStores);

                var stores = storeFinder.GetStores();
                foreach (var store in stores)
                {
                    alertList.AddRange(Alerts.Where(m => m.DepartmentId == store.DepartmentId));
                }
            }
            else {
                alertList = Alerts;
            }
            return alertList;
        }
    }
}
