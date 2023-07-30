using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Models.ViewModels;
using MODAMS.DataAccess.Data;
using MODAMS.Utility;
using NuGet.ContentModel;
using MODAMS.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Index()
        {
            List<vwAlert> Alerts = await GetAlerts();
            return View(Alerts);
        }
        public string GetAlertCount() {
            List<vwAlert> Alerts = GetAlerts().GetAwaiter().GetResult();
            return Alerts.Count().ToString();
        }
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
                        Model = asset.Model,
                        Name = asset.Name,
                        Alert = "Missing Documents",
                        EmployeeId = asset.Store.Department.EmployeeId
                    };
                    Alerts.Add(alert);
                }
            }

            if (User.IsInRole("User"))
            {
                _employeeId = _func.GetSupervisorId(_employeeId);
            }

            if (User.IsInRole("StoreOwner") || User.IsInRole("User"))
            {
                Alerts = Alerts.Where(m => m.EmployeeId == _employeeId).ToList();
            }

            return Alerts;
        }
    }
}
