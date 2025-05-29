using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Utility;
using System.Diagnostics;
using System.Text;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.ApplicationServices.IServices;
using System.Globalization;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private int _employeeId;
        private readonly bool _isSomali;

        public readonly IHomeService _homeService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, IAMSFunc func,
            UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment, IHomeService homeService)
        {
            _logger = logger;
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
            _isSomali = CultureInfo.CurrentCulture.Name == "so";

            _homeService = homeService;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _homeService.GetIndexAsync();
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new DashboardDTO());
            }
        }
        [HttpGet]
        public async Task<IActionResult> Profile(int? id)
        {
            if (id != null)
            {
                _employeeId = (int)id;
            }

            var result = await _homeService.GetProfileAsync(_employeeId);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new ProfileDataDTO());
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfile(ProfileDataDTO form)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = _isSomali? "Fadlan buuxi dhammaan meelaha khasabka ah!" : "Please complete all the mandatory fields!";
                return RedirectToAction("Profile", "Employees");
            }

            var result = await _homeService.SaveProfileAsync(form);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = _isSomali? "Macluumaadkaaga waa la cusbooneysiiyay si guul leh!" : "Profile updated successfuly!";
                return RedirectToAction("Profile", "Home", new { id = dto.Id });
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Profile", "Home", new { id = dto.Id });
            }
        }
        public async Task<IActionResult> ResetPassword()
        {
            string emailAddress = await _func.GetEmployeeEmailAsync();
            var user = await _userManager.FindByEmailAsync(emailAddress);
            if (user == null)
            {
                TempData["error"] = _isSomali? "Isticmaalaha lama helin!" : "User not found!";
                return RedirectToAction("Profile", "Employees");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Generate the callback URL in the controller
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme
            );

            var result = await _homeService.ResetPasswordAsync(emailAddress, callbackUrl);

            if (!result.IsSuccess)
            {
                var error = _isSomali? "Khalad ayaa dhacay inta la dib-u-dejinayay erayga sirta." : "An error occurred while resetting the password.";
                TempData["error"] = result.ErrorMessage ?? error;
                return RedirectToAction("Profile", "Employees");
            }

            TempData["success"] = _isSomali? "Tilmaamaha dib-u-dejinta erayga sirta waa la diray!" : "Password reset instructions sent!";
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

            var result = await _homeService.UploadPictureAsync(_employeeId, file, wwwRootPath);

            if (!result.IsSuccess)
            {
                var error = _isSomali ? "Khalad ayaa dhacay inta la soo gelinayay faylka." : "File upload failed or an error occurred.";
                TempData["error"] = result.ErrorMessage ?? error;
                return RedirectToAction("Profile", "Home");
            }

            TempData["success"] = _isSomali? "Sawirka macluumaadkaaga ayaa si guul leh loo soo geliyay!" : "Profile picture uploaded successfully!";
            return RedirectToAction("Profile", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> GetProfileData()
        {
            var result = await _homeService.GetProfileDataAsync();

            if (!result.IsSuccess)
            {
                return Json(new { status = "error", message = result.ErrorMessage });
            }

            if (result.Value == null)
            {
                return Json(new { status = "empty", message = _isSomali? "Macluumaadkaaga lama heli karo." : "Profile not available." });
            }

            return Json(new { status = "success", data = result.Value });
        }
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var result = await _homeService.GetNotificationsAsync();

            if (!result.IsSuccess)
            {
                return Json(new { status = "error", message = result.ErrorMessage });
            }

            if (result.Value == null || !result.Value.Any())
            {
                return Json(new { status = "empty", message = _isSomali? "Digniino lama heli karo." : "No notifications available." });
            }

            return Json(new { status = "success", data = result.Value });
        }
        public IActionResult Settings()
        {
            return View();
        }
        public async Task<IActionResult> GlobalSearch(string barcode)
        {
            // Call the service to perform the search
            var result = await _homeService.SearchTransferOrAssetAsync(barcode);

            if (!result.IsSuccess)
            {
                var error = _isSomali ? "Khalad ayaa dhacay inta la baarayay" : "An error occurred during search";
                TempData["error"] = result.ErrorMessage ?? error;
                return View(new GlobalSearchDTO());
            }

            // Check if the transfer was found and redirect to PreviewTransfer
            if (result.Value.TransferId > 0)
            {
                return RedirectToAction("PreviewTransfer", "Transfers", new { id = result.Value.TransferId });
            }

            // Otherwise, return the view with the asset search result
            return View(result.Value);
        }
        public IActionResult UnderConstruction()
        {
            return View();
        }
        public async Task<IActionResult> NotificationDirector(int id)
        {
            // Call the service to handle the notification
            var result = await _homeService.HandleNotificationAsync(id);
            var dto = result.Value;

            if (!result.IsSuccess)
            {
                var error = _isSomali ? "Khalad ayaa dhacay inta la farsameynayay digniinta." : "An error occurred while processing the notification.";
                TempData["error"] = result.ErrorMessage ?? error;
                return View();
            }

            // Redirect based on the DTO from the service
            return RedirectToAction(dto.Action, dto.Controller, new { area = dto.Area, id = dto.TargetRecordId });
        }
        [HttpGet]
        public async Task<IActionResult> AllNotifications(int id)
        {
            var result = await _homeService.GetAllNotificationsAsync(id);
            var dto = result.Value;

            if (!result.IsSuccess)
            {
                TempData["error"] = result.ErrorMessage;
                return View(new List<NotificationDTO>());
            }
            else {
                return View(dto);
            }
        }
        public async Task<IActionResult> ClearNotifications()
        {
            var result = await _homeService.ClearAllNotificationsAsync();
            var dto = result.Value;

            if (!result.IsSuccess)
            {
                TempData["error"] = result.ErrorMessage;
            }
            else {
                TempData["success"] = _isSomali? "Dhammaan digniinaha waa la tirtiray si guul leh!" : "All Notifications cleared successfuly!";
            }

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> Newsfeed()
        {
            var result = await _homeService.GetListOfNewsfeedAsync();
            var dto = result.Value;
            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                return View(new List<NewsFeed>());
            }
            
        }
    }
}