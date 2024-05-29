using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Utility;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text;
using MODAMS.Models.ViewModels.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;


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
            var categoryAssets = _db.vwCategoryAssets.ToList();
            var newsFeed = _db.NewsFeed.OrderByDescending(m=>m.TimeStamp).Take(5).ToList();
            
            var dto = new dtoDashboard()
            {
                CategoryAssets = categoryAssets,
                NewsFeed = newsFeed
            };

            dto.StoreCount = _db.Stores.Count();
            dto.UserCount = _db.Users.Count();
            dto.CurrentValue = GetCurrentValue();
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Profile(int? id)
        {
            if (id != null)
            {
                _employeeId = (int)id;
            }
            var profile = new dtoProfileData();

            var employeeInDb = await _db.Employees.Where(m => m.Id == _employeeId).SingleOrDefaultAsync();
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
                    Department = await _func.GetDepartmentName(_employeeId),
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

                _func.LogNewsFeed(await _func.GetEmployeeName() + " updated his profile", "Users", "Home", "Profile", rec.Id);
            }
            else
            {
                TempData["error"] = "Record not found!";
                return RedirectToAction("Profile", "Home");
            }
            TempData["success"] = "Profile updated successfuly!";
            return RedirectToAction("Profile", "Home");
        }

        public async Task<IActionResult> ResetPassword(int id = 0)
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

            if (id > 0)
            {
                return RedirectToAction("Settings", "Home");
            }
            else
            {
                return RedirectToAction("Profile", "Home");
            }


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

            if (file == null || file.Length == 0)
            {
                TempData["error"] = "Please select a file to upload!";
                return RedirectToAction("Profile", "Home");
            }

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

                    _func.LogNewsFeed(await _func.GetEmployeeName() + " uploaded a profile picture", "Users", "Home", "Profile", employee.Id);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred while uploading the file: {ex.Message}";
                return RedirectToAction("Profile", "Home");
            }
            return RedirectToAction("Profile", "Home");

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

        public string GetNotifications()
        {
            string sResult = "No Records Found";
            if (User.IsInRole("Administrator") || User.IsInRole("SeniorManagement"))
                return "No Records Found";

            var notifications = _db.Notifications.Where(m => m.EmployeeTo == _employeeId)
                .Include(m => m.NotificationSection).OrderByDescending(m => m.DateTime).ToList();

            var dto = new List<dtoNotification>();

            if (notifications.Count > 0)
            {
                foreach (var notification in notifications)
                {
                    var notif = new dtoNotification()
                    {
                        Id = notification.Id,
                        DateTime = notification.DateTime,
                        EmployeeFrom = notification.EmployeeFrom,
                        EmployeeTo = notification.EmployeeTo,
                        Subject = notification.Subject,
                        Message = notification.Message,
                        TargetRecordId = notification.TargetRecordId,
                        IsViewed = notification.IsViewed,
                        NotificationSectionId = notification.NotificationSectionId,
                        Area = notification.NotificationSection.area,
                        Controller = notification.NotificationSection.controller,
                        Action = notification.NotificationSection.action,
                        ImageUrl = _func.GetProfileImage(notification.EmployeeFrom)
                    };
                    dto.Add(notif);
                }
                sResult = JsonConvert.SerializeObject(dto);
            }
            return sResult;
        }

        public IActionResult Settings()
        {
            return View();
        }

        public async Task<IActionResult> GlobalSearch(string barcode)
        {
            var transfer = await _db.Transfers.FirstOrDefaultAsync(m => m.TransferNumber == barcode);
            if (transfer != null)
            {
                return RedirectToAction("PreviewTransfer", "Transfers", new { id = transfer.Id });
            }

            
            var asset = await _func.AssetGlobalSearch(barcode.Trim());
            dtoGlobalSearch dto = new dtoGlobalSearch();

            if (asset != null)
            {
                var assetPicture = _db.AssetPictures.Where(m => m.AssetId == asset.Id).FirstOrDefault();
                dto.Asset = asset;
                if (assetPicture != null)
                {
                    dto.AssetPicture = assetPicture;
                }
            }
            return View(dto);
        }

        public IActionResult UnderConstruction()
        {
            return View();
        }

        public IActionResult NotificationDirector(int id)
        {
            var notification = _db.Notifications.Where(m => m.Id == id)
                .Include(m => m.NotificationSection)
                .FirstOrDefault();

            if (notification == null)
            {
                return View();
            }
            notification.IsViewed = true;
            _db.SaveChanges();
            return RedirectToAction(notification.NotificationSection.action,
                notification.NotificationSection.controller,
                new { area = notification.NotificationSection.area, id = notification.TargetRecordId });

        }

        [HttpGet]
        public IActionResult AllNotifications(int id)
        {
            var notifications = _db.Notifications.Where(m => m.EmployeeTo == _employeeId)
                .Include(m => m.NotificationSection).OrderByDescending(m => m.DateTime).ToList();

            var dto = new List<dtoNotification>();

            if (notifications.Count > 0)
            {
                foreach (var notification in notifications)
                {
                    var notif = new dtoNotification()
                    {
                        Id = notification.Id,
                        DateTime = notification.DateTime,
                        EmployeeFrom = notification.EmployeeFrom,
                        EmployeeTo = notification.EmployeeTo,
                        Subject = notification.Subject,
                        Message = notification.Message,
                        TargetRecordId = notification.TargetRecordId,
                        IsViewed = notification.IsViewed,
                        NotificationSectionId = notification.NotificationSectionId,
                        Area = notification.NotificationSection.area,
                        Controller = notification.NotificationSection.controller,
                        Action = notification.NotificationSection.action,
                        ImageUrl = _func.GetProfileImage(notification.EmployeeFrom)
                    };
                    dto.Add(notif);
                }
            }
            return View(dto);
        }
        public async Task<IActionResult> ClearNotifications()
        {

            var notifications = _db.Notifications.Where(m => m.EmployeeTo == _employeeId).ToList();
            _db.Notifications.RemoveRange(notifications);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Newsfeed() {
            var newsFeed = _db.NewsFeed.OrderByDescending(m => m.TimeStamp).ToList();
            return View(newsFeed);
        }
        private decimal GetCurrentValue()
        {
            var assets = _db.Assets.Select(m => new { m.Id }).ToList();
            decimal currentValue = 0;
            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    currentValue += _func.GetDepreciatedCost(asset.Id);
                }
            }
            return currentValue;
        }
    }
}