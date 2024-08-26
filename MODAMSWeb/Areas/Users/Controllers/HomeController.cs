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
using Kendo.Mvc.UI;


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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categoryAssets = await _db.vwCategoryAssets.ToListAsync();
            var newsFeed = await _db.NewsFeed.OrderByDescending(m => m.TimeStamp).Take(5).ToListAsync();

            var dto = new dtoDashboard()
            {
                CategoryAssets = categoryAssets,
                NewsFeed = newsFeed
            };

            dto.StoreCount = await _db.Stores.CountAsync();
            dto.UserCount = await _db.Users.CountAsync();
            dto.CurrentValue = await GetCurrentValueAsync();

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
                    Department = await _func.GetDepartmentNameAsync(_employeeId),
                    RoleName = await _func.GetRoleNameAsync(_employeeId),
                    SupervisorEmployeeId = employeeInDb.SupervisorEmployeeId,
                    SupervisorName = await _func.GetSupervisorNameAsync(_employeeId),
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
            var rec = await _db.Employees.Where(m => m.Id == form.Id).SingleOrDefaultAsync();
            if (rec != null)
            {
                rec.FullName = form.FullName;
                rec.JobTitle = form.JobTitle;
                rec.Phone = form.Phone;
                rec.CardNumber = form.CardNumber;
                await _db.SaveChangesAsync();

                await _func.LogNewsFeedAsync(await _func.GetEmployeeNameAsync() + " updated his profile", "Users", "Home", "Profile", rec.Id);
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
            string emailAddress = await _func.GetEmployeeEmailAsync();
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

                    await _func.LogNewsFeedAsync(await _func.GetEmployeeNameAsync() + " uploaded a profile picture", "Users", "Home", "Profile", employee.Id);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An error occurred while uploading the file: {ex.Message}";
                return RedirectToAction("Profile", "Home");
            }
            return RedirectToAction("Profile", "Home");

        }
        public async Task<string> GetProfileData()
        {
            string sResult = "No Records Found";
            var profileData = new List<dtoProfileData>();
            string sEmail = await _func.GetEmployeeEmailAsync();

            var employeeInDb = await _db.Employees.Where(m => m.Email == sEmail).SingleOrDefaultAsync();
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

        public async Task<string> GetNotifications()
        {
            string sResult = "No Records Found";

            // Check roles and return early if necessary
            if (User.IsInRole("Administrator") || User.IsInRole("SeniorManagement"))
                return "No Records Found";

            // Retrieve the notifications from the database
            var notifications = await _db.Notifications
                .Where(m => m.EmployeeTo == _employeeId)
                .Include(m => m.NotificationSection)
                .OrderByDescending(m => m.DateTime)
                .ToListAsync();

            // If there are notifications, process them
            if (notifications.Count > 0)
            {
                // Create a list of dtoNotification without the ImageUrl
                var dto = notifications.Select(m => new dtoNotification()
                {
                    Id = m.Id,
                    DateTime = m.DateTime,
                    EmployeeFrom = m.EmployeeFrom,
                    EmployeeTo = m.EmployeeTo,
                    Subject = m.Subject,
                    Message = m.Message,
                    TargetRecordId = m.TargetRecordId,
                    IsViewed = m.IsViewed,
                    NotificationSectionId = m.NotificationSectionId,
                    Area = m.NotificationSection.area,
                    Controller = m.NotificationSection.controller,
                    Action = m.NotificationSection.action
                    // ImageUrl will be set later
                }).ToList();

                // Asynchronously fetch the ImageUrl for each dtoNotification
                foreach (var notification in dto)
                {
                    notification.ImageUrl = await _func.GetProfileImageAsync(notification.EmployeeFrom);
                }

                // Serialize the dto list to JSON
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


            var asset = await _func.AssetGlobalSearchAsync(barcode.Trim());
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

        public async Task<IActionResult> NotificationDirector(int id)
        {
            var notification = await _db.Notifications.Where(m => m.Id == id)
                .Include(m => m.NotificationSection)
                .FirstOrDefaultAsync();

            if (notification == null)
            {
                return View();
            }
            notification.IsViewed = true;
            await _db.SaveChangesAsync();

            return RedirectToAction(notification.NotificationSection.action,
                notification.NotificationSection.controller,
                new { area = notification.NotificationSection.area, id = notification.TargetRecordId });
        }

        [HttpGet]
        public async Task<IActionResult> AllNotifications(int id)
        {
            var notifications = await _db.Notifications
                .Where(m => m.EmployeeTo == _employeeId)
                .Include(m => m.NotificationSection)
                .OrderByDescending(m => m.DateTime)
                .Select(notification => new dtoNotification
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
                    Action = notification.NotificationSection.action
                    // ImageUrl will be set later
                })
                .ToListAsync();

            // Now, asynchronously set the ImageUrl for each notification
            foreach (var notification in notifications)
            {
                notification.ImageUrl = await _func.GetProfileImageAsync(notification.EmployeeFrom);
            }

            return View(notifications);
        }


        public async Task<IActionResult> ClearNotifications()
        {
            var notifications = await _db.Notifications.Where(m => m.EmployeeTo == _employeeId).ToListAsync();
            _db.Notifications.RemoveRange(notifications);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Newsfeed()
        {
            var newsFeed = await _db.NewsFeed.OrderByDescending(m => m.TimeStamp).ToListAsync();
            return View(newsFeed);
        }
        
        private async Task<decimal> GetCurrentValueAsync()
        {
            List<assetDto> assets = await _db.Assets.Select(m => new assetDto()
            {    
                Id = m.Id,
                Cost = m.Cost,
                LifeSpan = m.SubCategory.LifeSpan,
                RecieptDate = m.RecieptDate
            }).ToListAsync();


            decimal currentValue = 0;

            foreach (var asset in assets)
            {
                currentValue += CalculateDepreciatedCost(asset);
            }

            return currentValue;
        }
        
        private decimal CalculateDepreciatedCost(assetDto asset)
        {
            //(Cost / LifeSpan_months) * (LifeSpan_months - Age)
            decimal depreciatedCost = 0;

            if (asset != null)
            {
                int nLifeSpan = asset.LifeSpan;
                decimal cost = asset.Cost;

                if (asset.RecieptDate != null)
                {
                    DateTimeOffset date1 = (DateTimeOffset)asset.RecieptDate;
                    DateTimeOffset date2 = DateTime.Now;

                    // Find the difference between two dates in months
                    int age = (date2.Year - date1.Year) * 12 + date2.Month - date1.Month;

                    depreciatedCost = (cost / nLifeSpan) * (nLifeSpan - age);
                }
            }

            if (depreciatedCost < 0)
                depreciatedCost = 0;

            return depreciatedCost;
        }

        private class assetDto
        {
            public int Id { get; set; }
            public int LifeSpan { get; set; }
            public decimal Cost { get; set; }
            public DateTime? RecieptDate { get; set; }
        }
    }
}