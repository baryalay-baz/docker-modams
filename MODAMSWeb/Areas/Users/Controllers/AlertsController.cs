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
using MODAMS.ApplicationServices;


namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class AlertsController : Controller
    {
        
        private readonly IAlertService _alertService;
        
        public AlertsController(ApplicationDbContext db, IAMSFunc func, IAlertService alertService)
        {
            _alertService = alertService;
        }

        public async Task<IActionResult> Index(int? departmentId = 0)
        {
            var result = await _alertService.GetIndexAsync(departmentId);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                return View(new AlertsDTO());
            }
        }
        public async Task<string> GetAlertCount() {
            var result = await _alertService.GetAlertCountAsync();
            var count = result.Value;

            if (result.IsSuccess)
            {
                return count.ToString();
            }
            else {
                return "0";
            }
        }

    }
}
