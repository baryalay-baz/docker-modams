using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MODAMS.DataAccess.Data;
using MODAMS.Models.ViewModels;
using MODAMS.Utility;
using Newtonsoft.Json;
using System;
using System.Text.Encodings.Web;
using System.Text;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Authorize]
    [Area("Users")]
    public class EmployeesController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public EmployeesController(ILogger<HomeController> logger, ApplicationDbContext db, IAMSFunc func,
            UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _logger = logger;
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [Authorize(Roles = "User, StoreOwner, SeniorManagement, Administrator")]
        public IActionResult Index()
        {
            var employees = _db.vwEmployees.ToList();

            if (User.IsInRole(SD.Role_StoreOwner)) {
                employees = employees.Where(m=>m.SupervisorEmployeeId == _employeeId).ToList();
            }

            return View(employees);
        }

        public IActionResult Profile(int? id) {
            if (id != null)
            {
                _employeeId = (int)id;
            }
            var profile = new dtoProfileData();

            var employeeInDb = _db.Employees.Where(m => m.Id == _employeeId).SingleOrDefault();
            if (employeeInDb != null)
            {
                profile = new dtoProfileData()
                {
                    Id = employeeInDb.Id,
                    FullName = employeeInDb.FullName,
                    JobTitle = employeeInDb.JobTitle,
                    Email = employeeInDb.Email,
                    Phone = employeeInDb.Phone,
                    ImageUrl = employeeInDb.ImageUrl,
                    Department = _func.GetDepartmentName(_employeeId),
                    RoleName = _func.GetRoleName(_employeeId),
                    SupervisorEmployeeId = employeeInDb.SupervisorEmployeeId,
                    SupervisorName = _func.GetSuperpervisorName(_employeeId),
                    CardNumber = employeeInDb.CardNumber
                };
            }
            
            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfile(dtoProfileData form) {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please complete all the mandatory fields!";
                return RedirectToAction("Profile", "Employees");
            }
            var rec = _db.Employees.Where(m=>m.Id==form.Id).SingleOrDefault();
            if (rec != null)
            {
                rec.FullName = form.FullName;
                rec.JobTitle = form.JobTitle;
                rec.Phone = form.Phone;
                rec.CardNumber = form.CardNumber;
                await _db.SaveChangesAsync();
            }
            else {
                TempData["error"] = "Record not found!";
                return RedirectToAction("Profile", "Employees");
            }
            TempData["success"] = "Profile updated successfuly!";
            return RedirectToAction("Profile", "Employees");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword()
        {
            if (ModelState.IsValid)
            {
                string emailAddress = _func.GetEmployeeEmail();
                var user = await _userManager.FindByEmailAsync(emailAddress);
                if (user == null)
                {
                    TempData["error"] = "User not found!";
                    return RedirectToAction("Profile", "Employees");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                
                var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                string shortmessage = "Password reset instructions have been received for your account, " +
                    "click the button below to follow the instructions!";

                string message = "";

                if (callbackUrl != null)
                {
                    message = _func.FormatMessage("Reset Password", shortmessage,
                        emailAddress, HtmlEncoder.Default.Encode(callbackUrl), "Reset Password");
                }
                else {
                    message = _func.FormatMessage("Reset Password", shortmessage,
                        emailAddress, HtmlEncoder.Default.Encode("./"), "Reset Password");
                }

                await _emailSender.SendEmailAsync(
                    emailAddress,
                    "Reset Password",
                    message);

                TempData["success"] = "Please check  your email!";
                return RedirectToAction("Profile", "Employees");
            }

            return View();
        }

        

    }
}
