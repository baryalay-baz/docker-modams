using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Utility;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.VisualBasic;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, IAMSFunc func,
            UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Profile(int? id)
        {
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
                    SupervisorName = _func.GetSupervisorName(_employeeId),
                    CardNumber = employeeInDb.CardNumber
                };
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfile(dtoProfileData form)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please complete all the mandatory fields!";
                return RedirectToAction("Profile", "Employees");
            }
            var rec = _db.Employees.Where(m => m.Id == form.Id).SingleOrDefault();
            if (rec != null)
            {
                rec.FullName = form.FullName;
                rec.JobTitle = form.JobTitle;
                rec.Phone = form.Phone;
                rec.CardNumber = form.CardNumber;
                await _db.SaveChangesAsync();
            }
            else
            {
                TempData["error"] = "Record not found!";
                return RedirectToAction("Profile", "Home");
            }
            TempData["success"] = "Profile updated successfuly!";
            return RedirectToAction("Profile", "Home");
        }

        public async Task<IActionResult> ResetPassword()
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
            else
            {
                message = _func.FormatMessage("Reset Password", shortmessage,
                    emailAddress, HtmlEncoder.Default.Encode("./"), "Reset Password");
            }


            await _emailSender.SendEmailAsync(
                emailAddress,
                "Reset Password",
                message);

            TempData["success"] = "Password reset instructions sent!";
            return RedirectToAction("Profile", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile? file)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = _employeeId.ToString() + Path.GetExtension(file.FileName);
                string facesPath = Path.Combine(wwwRootPath, @"assets\images\faces");

                try
                {
                    using (var fileStream = new FileStream(Path.Combine(facesPath, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    var employee = _db.Employees.Where(m => m.Id == _employeeId).FirstOrDefault();

                    if (employee != null)
                    {
                        employee.ImageUrl = "/assets/images/faces/" + fileName;
                        await _db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Error occured! <br /> " + ex.Message;
                    return RedirectToAction("Profile", "Home");
                }
                return RedirectToAction("Profile", "Home");
            }
            else {
                TempData["error"] = "Please select a file to upload!";
                return RedirectToAction("Profile", "Home");
            }
        }
        public string GetProfileData()
        {
            string sResult = "No Records Found";
            var profileData = new List<dtoProfileData>();
            string sEmail = _func.GetEmployeeEmail();

            var employeeInDb = _db.Employees.Where(m => m.Email == sEmail).SingleOrDefault();
            if (employeeInDb != null)
            {
                var profile = new dtoProfileData()
                {
                    Id = employeeInDb.Id,
                    FullName = employeeInDb.FullName,
                    JobTitle = employeeInDb.JobTitle,
                    Email = employeeInDb.Email,
                    Phone = employeeInDb.Phone,
                    ImageUrl = employeeInDb.ImageUrl,
                };
                profileData.Add(profile);

                sResult = JsonConvert.SerializeObject(profileData);
            }
            else
            {
                TempData["error"] = "Record not found!";
            }
            return sResult;
        }
    }
}