using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using System.Globalization;
using Telerik.Reporting.Services;

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

        private readonly bool _isSomali;
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

            _isSomali = CultureInfo.CurrentUICulture.Name == "so";
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ReportingDTO dto = new ReportingDTO();
            dto = await PopulateDTO(dto);

            return View(dto);
        }
        public async Task<IActionResult> ReportViewerExternal(string Type, int id)
        {
            var dto = new ReportingDTO();
            if (Type == "PrintVoucher")
            {
                dto = await PopulateDTO(dto);
                dto.ReportId = "TransferVoucher";
                dto.TransferId = id;
            }
            return View("ReportViewer", dto);
        }
        [HttpPost]
        public async Task<IActionResult> ReportViewer(ReportingDTO dto)
        {
            dto = await PopulateDTO(dto);
            return View(dto);
        }
        private async Task<ReportingDTO> PopulateDTO(ReportingDTO dto)
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
                Text = _isSomali ? m.NameSo : m.Name,
                Value = m.Id.ToString()
            });
            var categories = await _db.Categories.Select(m => new SelectListItem
            {
                Text = _isSomali ? m.CategoryNameSo : m.CategoryName,
                Value = m.Id.ToString()
            }).ToListAsync();

            var subCategories = await _db.SubCategories.Select(m => new SelectListItem
            {
                Text = _isSomali ? m.SubCategoryNameSo : m.SubCategoryName,
                Value = m.Id.ToString()
            }).ToListAsync();
            var assetStatuses = await _db.AssetStatuses.Select(m => new SelectListItem
            {
                Text = _isSomali ? m.StatusNameSo : m.StatusName,
                Value = m.Id.ToString()
            }).ToListAsync();
            var assetConditions = await _db.Conditions.Select(m => new SelectListItem
            {
                Text = _isSomali ? m.ConditionNameSo : m.ConditionName,
                Value = m.Id.ToString()
            }).ToListAsync();
            var donors = await _db.Donors.Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            }).ToListAsync();

            dto.AssetStores = storeSelectList;
            dto.AssetStatuses = assetStatuses;
            dto.Categories = categories;
            dto.SubCategories = subCategories;
            dto.Conditions = assetConditions;
            dto.Donors = donors;

            //Populate Transfer Report
            dto.TransferStores = allStores.ToList().Select(m => new SelectListItem
            {
                Text = _isSomali ? m.NameSo : m.Name,
                Value = m.Id.ToString()
            });
            dto.TransferStatuses = await _db.TransferStatuses.Select(m => new SelectListItem
            {
                Text = _isSomali ? m.StatusSo : m.Status,
                Value = m.Id.ToString()
            }).ToListAsync();

            //Populate Disposal= Report
            var disposalTypes = await _db.DisposalTypes.ToListAsync();
            dto.DisposalTypes = disposalTypes.ToList().Select(m => new SelectListItem
            {
                Text = _isSomali ? m.TypeSo : m.Type,
                Value = m.Id.ToString()
            });

            return dto;
        }

    }
}
