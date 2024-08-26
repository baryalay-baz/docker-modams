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
            List<vwAlert> DocumentAlerts = await GetDocumentAlerts();
            List<vwAlert>MissingDataAlerts = await GetMissingDataAlerts();

            var dto = new dtoAlerts();

            var departments = DocumentAlerts.Select(m => new { m.DepartmentId, m.Department }).Distinct();

            if (departmentId > 0) {
                DocumentAlerts = DocumentAlerts.Where(m=>m.DepartmentId == departmentId).ToList();
                MissingDataAlerts = MissingDataAlerts.Where(m=>m.DepartmentId==departmentId).ToList();
            }

            dto.Alerts.AddRange(DocumentAlerts);
            dto.Alerts.AddRange(MissingDataAlerts);
            
            var departmentList = departments.Select(m => new SelectListItem
            {
                Text = m.Department,
                Value = m.DepartmentId.ToString(),
                Selected = (m.DepartmentId == departmentId)
            });

            dto.DepartmentId = departmentId > 0 ? (int)departmentId : 0;
            dto.DepartmentList = departmentList;

            return View(dto);
        }
        public string GetAlertCount() {
            List<vwAlert> Alerts = GetDocumentAlerts().GetAwaiter().GetResult();
            return Alerts.Count.ToString();
        }

        //Alerts Retrieval
        private async Task<List<vwAlert>> GetDocumentAlerts()
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
                string documentName = "";
                foreach (var documentType in documentTypes)
                {
                    documentName = documentType.Name;
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
                        Description = documentName + " is not uploaded!",
                        EmployeeId = asset.Store.Department.EmployeeId
                    };
                    Alerts.Add(alert);
                }
            }

            var alertList = new List<vwAlert>();

            if (User.IsInRole("User"))
            {
                _employeeId = await _func.GetSupervisorIdAsync(_employeeId);
            }


            if (User.IsInRole("StoreOwner") || User.IsInRole("User"))
            {
                var allStores = _db.vwStores.ToList();
                int DepartmentId = await _func.GetDepartmentIdAsync(_employeeId);
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
        private async Task<List<vwAlert>> GetMissingDataAlerts() {
            _employeeId = User.IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;

            var assets = await _db.Assets
                .Include(m => m.SubCategory.Category)
                .Include(m => m.Store.Department)
                .ToListAsync();

            var alerts = new List<vwAlert>();

            foreach (var asset in assets) {
                bool blnCheck = false;
                string sDataColumn = "";

                if (asset.Make.Length < 2)
                {
                    blnCheck = true;
                    sDataColumn = "Make";
                }
                if (asset.Model.Length < 2)
                {
                    blnCheck = true;
                    sDataColumn = "Model";
                }
                if (asset.Year.Length < 2)
                {
                    blnCheck = true;
                    sDataColumn = "Year";
                }
                if (asset.Make.Length < 2)
                {
                    blnCheck = true;
                    sDataColumn = "Make";
                }
                if (asset.Name.Length < 2)
                {
                    blnCheck = true;
                    sDataColumn = "Asset Name";
                }
                if (asset.SubCategory.Category.CategoryName == "Vehicle")
                {
                    if (asset.Chasis.Length < 2)
                    {
                        blnCheck = true;
                        sDataColumn = "Chasis Number";
                    }
                    if (asset.Engine.Length < 2)
                    {
                        blnCheck = true;
                        sDataColumn = "Engine Number";
                    }
                    if (asset.Plate.Length < 2)
                    {
                        blnCheck = true;
                        sDataColumn = "Plate Number";
                    }
                }
                else { 
                    if(asset.SerialNo.Length < 2)
                    {
                        blnCheck = true;
                        sDataColumn = "Serial Number";
                    }
                }
                if (asset.Specifications.Length < 2)
                {
                    blnCheck = true;
                    sDataColumn = "Technical Specifications";
                }
                if (asset.Cost < 2)
                {
                    blnCheck = true;
                    sDataColumn = "Cost";
                }
                if (!DateTime.TryParse(asset.PurchaseDate.ToString(), out DateTime purchaseDate))
                {
                    blnCheck = true;
                    sDataColumn = "Purchase Date";
                }
                if (!DateTime.TryParse(asset.RecieptDate.ToString(), out DateTime recieptDate))
                {
                    blnCheck = true;
                    sDataColumn = "Reciept Date";
                }
                if (blnCheck)
                {
                    var subCategoryName = asset.SubCategory?.SubCategoryName ?? "";
                    var departmentId = asset.Store?.Department?.Id ?? 0;
                    var departmentName = asset.Store?.Department?.Name ?? "";
                    var employeeId = asset.Store?.Department?.EmployeeId ?? 0;

                    var alert = new vwAlert
                    {
                        AssetId = asset.Id,
                        SubCategory = subCategoryName,
                        Make = asset.Make,
                        DepartmentId = departmentId,
                        Department = departmentName,
                        Name = asset.Name,
                        AlertType = "Missing Data",
                        Description = $"{sDataColumn} is not valid!",
                        EmployeeId = employeeId
                    };
                    alerts.Add(alert);
                }
            }

            var alertList = new List<vwAlert>();

            if (User.IsInRole("StoreOwner") || User.IsInRole("User"))
            {
                var allStores = _db.vwStores.ToList();
                int DepartmentId = await _func.GetDepartmentIdAsync(_employeeId);
                var storeFinder = new StoreFinder(DepartmentId, allStores);

                var stores = storeFinder.GetStores();
                foreach (var store in stores)
                {
                    alertList.AddRange(alerts.Where(m => m.DepartmentId == store.DepartmentId));
                }
            }
            else
            {
                alertList = alerts;
            }
            return alertList;
        }

        private bool IsDate(string datestring)
        {
            if (DateTime.TryParse(datestring, out DateTime date))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
