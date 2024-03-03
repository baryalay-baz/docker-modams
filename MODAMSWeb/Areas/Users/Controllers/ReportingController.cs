using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using NuGet.ContentModel;
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using Telerik.Reporting.Services;
using Telerik.Reporting.Services.AspNetCore;


namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class ReportingController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;
        private int _storeId;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportingController(ApplicationDbContext db, IAMSFunc func, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            //var assets = _db.Assets.Include(m => m.SubCategory).ThenInclude(m => m.Category)
            //    .Include(m => m.Store).ThenInclude(m => m.Department)
            //    .Include(m => m.AssetStatus).Include(m => m.Condition).ToList();

            var stores = _db.Stores.ToList().Select(m => new SelectListItem
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
            dtoReporting dto = new dtoReporting()
            {
                AssetStatuses = assetStatuses,
                Categories = categories,
                SubCategories = subCategories,
                Conditions = assetConditions,
                Stores = stores,
                Donors = donors
            };
            return View(dto);
        }

        [HttpGet]
        public IActionResult ReportViewer(int storeId = 0, int assetStatusId = 0, int categoryId = 0)
        {
            var reportPath = Path.Combine(_webHostEnvironment.WebRootPath, "Reports\\AssetReport.trdp");

            var reportPackager = new ReportPackager();
            using (var sourceStream = System.IO.File.OpenRead(reportPath))
            {
                var list = _db.Assets.ToList();
                var report = (Telerik.Reporting.Processing.Report)reportPackager.UnpackageDocument(sourceStream);
                report.DataSource = list;
            }

            return View();
        }
    }
}
