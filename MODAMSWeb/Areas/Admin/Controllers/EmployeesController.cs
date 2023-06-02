using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MODAMS.DataAccess.Data;
using MODAMS.Models.ViewModels;
using MODAMS.Models;
using MODAMS.Utility;
using System.Data;
using System.Text.Encodings.Web;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MODAMSWeb.Areas.Admin.Controllers
{
    [Authorize(Roles ="StoreOwner, SeniorManagement, Administrator")]
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

        public EmployeesController(ILogger<HomeController> logger, ApplicationDbContext db, IAMSFunc func,
            UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            _logger = logger;
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        [Authorize(Roles = "StoreOwner, SeniorManagement, Administrator")]
        public IActionResult Index()
        {
            var employees = _db.vwEmployees.ToList();

            if (User.IsInRole(SD.Role_StoreOwner))
            {
                employees = employees.Where(m => m.SupervisorEmployeeId == _employeeId).ToList();
            }

            return View(employees);
        }

        [Authorize(Roles = "StoreOwner, Administrator")]
        [HttpGet]
        public IActionResult CreateEmployee()
        {

            var employee = new dtoEmployee();

            var roleList = _db.Roles.ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Name
            });

            var employeeList = _db.Employees.ToList().Select(m => new SelectListItem
            {
                Text = m.FullName,
                Value = m.Id.ToString()
            });

            if (User.IsInRole("StoreOwner"))
            {
                roleList = roleList.Where(m => m.Text == "User");
            }
            else
            {
                roleList = roleList.Where(m => m.Text != "Administrator");
            }


            employee.Employee = new Employee();
            employee.Employees = employeeList;
            employee.RoleList = roleList;

            return View(employee);
        }

        [Authorize(Roles = "StoreOwner, Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(dtoEmployee form)
        {
            if (ModelState.IsValid)
            {
                var employee = await _db.Employees.FirstOrDefaultAsync(m => m.Email == form.Employee.Email);

                if (employee != null)
                {
                    TempData["error"] = "Employee aleady exists!";
                    return RedirectToAction("CreateEmployee", "Employees");
                }

                form.Employee.ImageUrl = "/assets/images/faces/profile_placeholder.png";
                form.Employee.IsActive = true;
                _db.Employees.Add(form.Employee);

                await _db.SaveChangesAsync();
                TempData["success"] = "Employee added successfuly!";
                SendRegistrationNotification(form.Employee.Email);
            }
            else
            {
                TempData["error"] = "All fields are mandatory!";
                return RedirectToAction("CreateEmployee", "Employees");
            }

            return RedirectToAction("Index", "Employees");
        }

        [Authorize(Roles = "StoreOwner, Administrator")]
        [HttpGet]
        public IActionResult EditEmployee(int id)
        {
            if (id == 0)
            {
                TempData["error"] = "Record not found!";
                return RedirectToAction("Index", "Employees");
            }
            var employeeDto = new dtoEmployee();
            var employee = _db.Employees.Where(m => m.Id == id).FirstOrDefault();

            if (employee != null)
            {
                employeeDto.Employee = employee;

                var currentRole = _func.GetRoleName(id);
                var roleList = _db.Roles.ToList().Select(m => new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Name
                });
                var employeeList = _db.Employees.ToList().Select(m => new SelectListItem
                {
                    Text = m.FullName,
                    Value = m.Id.ToString()
                });

                if (User.IsInRole("StoreOwner"))
                {
                    roleList = roleList.Where(m => m.Text == "User");
                }

                employeeDto.Employee = employee;
                employeeDto.Employees = employeeList;
                employeeDto.RoleList = roleList;
                employeeDto.roleId = currentRole;
            }
            else
            {
                RedirectToAction("Index", "Employees", new { area = "Admin" });
            }


            return View(employeeDto);
        }

        [Authorize(Roles = "StoreOwner, Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(dtoEmployee form)
        {
            if (ModelState.IsValid)
            {
                var employee = await _db.Employees.FirstOrDefaultAsync(m => m.Email == form.Employee.Email);

                if (employee == null)
                {
                    TempData["error"] = "Record not found!";
                    return RedirectToAction("Index", "Employees");
                }

                employee.FullName = form.Employee.FullName;
                employee.JobTitle = form.Employee.JobTitle;
                employee.CardNumber = form.Employee.CardNumber;
                employee.SupervisorEmployeeId = form.Employee.SupervisorEmployeeId;
                employee.Phone = form.Employee.Phone;

                var user = await _userManager.FindByEmailAsync(employee.Email);
                if (user != null)
                {
                    var role = await _userManager.GetRolesAsync(user);
                    string roleName = "User";

                    if (role != null)
                    {
                        roleName = role[0];
                    }

                    var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, form.roleId);
                    }
                }
                await _db.SaveChangesAsync();

                TempData["success"] = "Employee updated successfuly!";
            }
            else
            {
                TempData["error"] = "All fields are mandatory!";
                return RedirectToAction("CreateEmployee", "Employees");
            }

            return RedirectToAction("Index", "Employees", new { area = "Admin" });

        }

        [Authorize(Roles = "StoreOwner, Administrator")]
        public async Task<IActionResult> LockAccount(int id)
        {
            var employee = _db.Employees.Where(m => m.Id == id).FirstOrDefault();
            if (employee != null)
            {
                employee.IsActive = false;
                await _db.SaveChangesAsync();
                TempData["success"] = "Account locked successfuly!";
            }
            else
            {
                TempData["error"] = "Record not found!";
            }
            return RedirectToAction("Index", "Employees");
        }

        [Authorize(Roles = "StoreOwner, Administrator")]
        public async Task<IActionResult> UnlockAccount(int id)
        {
            var employee = _db.Employees.Where(m => m.Id == id).FirstOrDefault();
            if (employee != null)
            {
                employee.IsActive = true;
                await _db.SaveChangesAsync();
                TempData["success"] = "Account unlocked successfuly!";
            }
            else
            {
                TempData["error"] = "Record not found!";
            }
            return RedirectToAction("Index", "Employees");
        }

        [Authorize(Roles = "Administrator, SeniorManagement, StoreOwner, User")]
        [HttpGet]
        public async Task<string> GetFaces() {
            var sResult = "No Records Found";

            var faces = await _db.Employees.Select(m=> new {m.Id, m.ImageUrl}).ToListAsync();
            if(faces.Count>0) {
                sResult = JsonConvert.SerializeObject(faces);
            }
            return sResult;
        }


        private async void SendRegistrationNotification(string emailAddress)
        {

            var callbackUrl = Url.Page(
            "/Account/Register",
                    pageHandler: null,
                    values: new { area = "Identity", returnUrl = emailAddress },
                    protocol: Request.Scheme);

            string shortmessage = "A new account has been created for you at MOD Asset Management System, " +
                "please click the button below to follow the instructions!";

            string message = "";

            if (callbackUrl != null)
            {
                message = _func.FormatMessage("Account Registration", shortmessage,
                    emailAddress, HtmlEncoder.Default.Encode(callbackUrl), "Register here");
            }
            else
            {
                message = _func.FormatMessage("Reset Password", shortmessage,
                    emailAddress, HtmlEncoder.Default.Encode("./"), "Register here");
            }

            await _emailSender.SendEmailAsync(
                emailAddress,
                "Account Registration",
                message);
        }
    }
}
