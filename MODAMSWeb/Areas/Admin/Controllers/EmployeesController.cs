using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Utility;
using System.Data;
using System.Text.Encodings.Web;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Models.ViewModels;
using MODAMS.ApplicationServices.IServices;
using System.Globalization;

namespace MODAMSWeb.Areas.Admin.Controllers
{
    [Authorize(Roles = "StoreOwner, SeniorManagement, Administrator")]
    [Area("Admin")]
    public class EmployeesController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;

        private readonly IEmployeesService _employeesService;
        private readonly bool _isSomali;
        public EmployeesController(IEmployeesService employeesService, ILogger<HomeController> logger, ApplicationDbContext db, IAMSFunc func,
            UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            _employeesService = employeesService;

            _logger = logger;
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;

            _isSomali = CultureInfo.CurrentCulture.Name == "so";
        }

        [Authorize(Roles = "StoreOwner, SeniorManagement, Administrator")]
        public async Task<IActionResult> Index()
        {
            var result = await _employeesService.GetIndexAsync();
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new List<vwEmployees>());
            }
        }

        [Authorize(Roles = "StoreOwner, Administrator")]
        [HttpGet]
        public async Task<IActionResult> CreateEmployee()
        {
            var result = await _employeesService.GetCreateEmployeeAsync();
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Employees");
            }
        }

        [Authorize(Roles = "StoreOwner, Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(EmployeeDTO form)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = _isSomali? "Fadlan buuxi dhammaan goobaha khasabka ah" : "Please fill all the mandatory fields";
                form = await _employeesService.PopulateEmployeeDTOAsync(form);

                return View(form);
            }

            var result = await _employeesService.CreateEmployeeAsync(form);
            if (result.IsSuccess)
            {
                TempData["success"] = _isSomali? "Shaqaale si guul leh ayaa lagu daray!" : "Employee added successfuly!";

                // Pass the base URL and scheme to the service for generating the URL
                await _employeesService.SendRegistrationNotification(
                    form.Employee.Email,
                    Request.Host.Value,
                    Request.Scheme);

                return RedirectToAction("Index", "Employees");
            }

            form = await _employeesService.PopulateEmployeeDTOAsync(form);
            TempData["error"] = result.ErrorMessage;
            return View(form);
        }


        [Authorize(Roles = "StoreOwner, Administrator")]
        [HttpGet]
        public async Task<IActionResult> EditEmployee(int id)
        {
            var result = await _employeesService.GetEditEmployeeAsync(id);

            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            TempData["error"] = result.ErrorMessage;
            return RedirectToAction("Index", "Employees");
        }

        [Authorize(Roles = "StoreOwner, Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(EmployeeDTO form)
        {
            if (!ModelState.IsValid)
            {
                form = await _employeesService.PopulateEmployeeDTOAsync(form);
                return View(form);
            }

            var result = await _employeesService.EditEmployeeAsync(form);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = _isSomali? "Diiwaanka shaqaalaha si guul leh ayaa loo cusboonaysiiyay!" : "Employee record updated successfully!";
                return RedirectToAction("Index", "Employees");
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(dto);
            }
        }

        [Authorize(Roles = "StoreOwner, Administrator")]
        public async Task<IActionResult> LockAccount(int id)
        {
            var result = await _employeesService.LockAccountAsync(id);

            if (result.IsSuccess) {
                TempData["success"] = _isSomali? "Akoonka si guul leh ayaa loo xiray!" : "Account locked successfuly!";
                return RedirectToAction("Index", "Employees");
            }

            TempData["error"] = result.ErrorMessage;
            return RedirectToAction("Index", "Employees");

        }

        [Authorize(Roles = "StoreOwner, Administrator")]
        public async Task<IActionResult> UnlockAccount(int id)
        {
            var result = await _employeesService.UnLockAccountAsync(id);

            if (result.IsSuccess)
            {
                TempData["success"] = _isSomali? "Akoonka si guul leh ayaa loo furay!" : "Account unlocked successfuly!";
                return RedirectToAction("Index", "Employees");
            }

            TempData["error"] = result.ErrorMessage;
            return RedirectToAction("Index", "Employees");
        }

        [Authorize(Roles = "Administrator, SeniorManagement, StoreOwner, User")]
        [HttpGet]
        public async Task<string> GetFaces()
        {
            var result = await _employeesService.GetFacesAsync();
            var data = result.Value;

            if (result.IsSuccess)
            {
                return data;
            }
            else {
                return result.ErrorMessage;
            }
        }

    }
}
