using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using System.Globalization;
using Telerik.SvgIcons;

namespace MODAMSWeb.Areas.Admin.Controllers

{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class Settings : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;

        public Settings(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
        }

        public async Task<IActionResult> Index(int? nMonth, int? nYear)
        {
            var loginHistory = await _db.LoginHistory.Include(m => m.Employee)
                .OrderByDescending(m => m.TimeStamp).ToListAsync();

            var auditLog = await _db.AuditLog.Where(m => m.EmployeeId != null)
                .Include(m => m.Employee).OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            var deletedAssets = await _db.Assets.Where(m => m.AssetStatusId == 4)
                .Include(m=>m.Store).ThenInclude(m=>m.Department)
                .Include(m => m.SubCategory)
                .ThenInclude(m => m.Category).ToListAsync();


            int month = nMonth ?? DateTime.Now.Month;
            int year = nYear ?? DateTime.Now.Year;

            loginHistory = loginHistory
                    .Where(m => m.TimeStamp.Month == month && m.TimeStamp.Year == year)
                    .ToList();

            auditLog = auditLog
                .Where(m => m.Timestamp.Month == month && m.Timestamp.Year == year)
                .ToList();



            var dto = new dtoSettings()
            {
                auditLog = auditLog,
                loginHistory = loginHistory,
                deletedAssets = deletedAssets,
                Months = GetMonths(month),
                Years = GetYears(year),
                SelectedMonth = month,
                SelectedYear = year
            };

            return View(dto);
        }

        private List<SelectListItem> GetMonths(int nSelected)
        {
            var months = Enumerable.Range(1, 12).Select(i => new SelectListItem
            {
                Value = i.ToString(),
                Text = GetMonthName(i),
                Selected = (i == nSelected)
            }).ToList();
            return months;
        }

        private List<SelectListItem> GetYears(int nSelected)
        {

            var years = Enumerable.Range(2023, 5).Select(i => new SelectListItem
            {
                Value = i.ToString(),
                Text = i.ToString(),
                Selected = (i == nSelected)
            }).ToList();

            return years;
        }

        private string GetMonthName(int monthNumber)
        {
            if (monthNumber < 1 || monthNumber > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(monthNumber), "Month number should be between 1 and 12");
            }

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            DateTimeFormatInfo dateTimeFormat = cultureInfo.DateTimeFormat;
            string monthName = dateTimeFormat.GetMonthName(monthNumber);

            return monthName;
        }
    }
}
