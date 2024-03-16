using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using NuGet.ContentModel;
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using Telerik.Reporting.Services;
using Telerik.Reporting.Services.AspNetCore;
using Telerik.Reporting.Services.Engine;


namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class ReportingController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;
        private int _supervisorEmployeeId;
        private int _storeId;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IReportSourceResolver _reportResolver;
        public ReportingController(ApplicationDbContext db, IAMSFunc func, IWebHostEnvironment webHostEnvironment, IReportSourceResolver reportResolver)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _supervisorEmployeeId = _func.GetSupervisorId(_employeeId);
            _webHostEnvironment = webHostEnvironment;
            _reportResolver = reportResolver;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //var assets = _db.Assets.Include(m => m.SubCategory).ThenInclude(m => m.Category)
            //    .Include(m => m.Store).ThenInclude(m => m.Department)
            //    .Include(m => m.AssetStatus).Include(m => m.Condition).ToList();

            dtoReporting dto = new dtoReporting();
            dto = await PopulateDTO(dto);

            return View(dto);
        }
        public async Task<IActionResult> ReportViewerExternal(string Type, int id)
        {
            var dto = new dtoReporting();
            if (Type == "PrintVoucher")
            {
                dto = await PopulateDTO(dto);
                dto.ReportId = "TransferVoucher";
                dto.TransferId = id;
            }
            return View("ReportViewer", dto);
        }

        [HttpPost]
        public async Task<IActionResult> ReportViewer(dtoReporting dto)
        {
            dto = await PopulateDTO(dto);
            return View(dto);
        }

        private async Task<dtoReporting> PopulateDTO(dtoReporting dto)
        {
            //Populate Asset Report
            var stores = await _db.vwStores.OrderByDescending(m => m.TotalCount).ToListAsync();
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

            var storeSelectList = stores.ToList().Select(m => new SelectListItem
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

            dto.AssetStores = storeSelectList;
            dto.AssetStatuses = assetStatuses;
            dto.Categories = categories;
            dto.SubCategories = subCategories;
            dto.Conditions = assetConditions;
            dto.Donors = donors;

            //Populate Transfer Report
            dto.TransferStores = allStores.ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            });
            dto.TransferStatuses = _db.TransferStatuses.ToList().Select(m => new SelectListItem
            {
                Text = m.Status,
                Value = m.Id.ToString()
            });

            return dto;
        }
        
    }
}
