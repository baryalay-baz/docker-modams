using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;

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

        public IActionResult Index()
        {
            var loginHistory = _db.LoginHistory.Include(m => m.Employee)
                .OrderByDescending(m => m.TimeStamp).ToList();
            var auditLog = _db.AuditLog.Include(m => m.Employee)
                .OrderByDescending(m=>m.Timestamp).ToList();
            var dto = new dtoSettings()
            {
                auditLog = auditLog,
                loginHistory = loginHistory
            };

            return View(dto);
        }
    }
}
