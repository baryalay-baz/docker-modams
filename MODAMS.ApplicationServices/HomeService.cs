using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MODAMS.DataAccess.Data;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using MODAMS.Models;
using MODAMS.ApplicationServices.IServices;


namespace MODAMS.ApplicationServices
{
    public class HomeService : IHomeService
    {

        private readonly ILogger<HomeService> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly int _employeeId;
        public HomeService(ILogger<HomeService> logger, ApplicationDbContext db, IAMSFunc func,
            IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, IEmailSender emailSender, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _db = db;
            _func = func;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;

            _employeeId = _func.GetEmployeeId();
        }
        private bool IsInRole(string role) => _httpContextAccessor.HttpContext.User.IsInRole(role);

        public async Task<Result<DashboardDTO>> GetIndexAsync()
        {
            try
            {
                var categoryAssets = await _func.GetvwCategoryAssetsAsync();
                var newsFeed = await _db.NewsFeed.OrderByDescending(m => m.TimeStamp).Take(5).ToListAsync();

                var dto = new DashboardDTO()
                {
                    CategoryAssets = categoryAssets,
                    NewsFeed = newsFeed
                };

                dto.StoreCount = await _db.Stores.CountAsync();
                dto.UserCount = await _db.Users.CountAsync();
                dto.CurrentValue = await GetCurrentValueAsync();

                return Result<DashboardDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<DashboardDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<List<NotificationDTO>>> GetNotificationsAsync()
        {
            try
            {
                if (IsInRole("Administrator") || IsInRole("SeniorManagement"))
                {
                    return Result<List<NotificationDTO>>.Success(new List<NotificationDTO>());
                }

                // Retrieve the notifications from the database
                var notifications = await _db.Notifications
                    .Where(m => m.EmployeeTo == _employeeId)
                    .Include(m => m.NotificationSection)
                    .OrderByDescending(m => m.DateTime)
                    .ToListAsync();

                if (notifications == null)
                {
                    return Result<List<NotificationDTO>>.Success(new List<NotificationDTO>());
                }

                // Map to DTO and fetch profile images
                var dto = notifications.Select(m => new NotificationDTO()
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
                    Action = m.NotificationSection.action,
                    ImageUrl = null // To be set asynchronously
                }).ToList();

                // Fetch ImageUrl asynchronously
                foreach (var notification in dto)
                {
                    notification.ImageUrl = await _func.GetProfileImageAsync(notification.EmployeeFrom);
                }

                return Result<List<NotificationDTO>>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<List<NotificationDTO>>.Failure(ex.Message);
            }
        }
        public async Task<Result<ProfileDataDTO>> GetProfileDataAsync()
        {
            try
            {
                var profileData = new List<ProfileDataDTO>();
                string sEmail = await _func.GetEmployeeEmailAsync();

                var employeeInDb = await _db.Employees.Where(m => m.Email == sEmail).FirstOrDefaultAsync();

                if (employeeInDb == null)
                {
                    return Result<ProfileDataDTO>.Success(new ProfileDataDTO());
                }

                var profile = new ProfileDataDTO()
                {
                    Id = employeeInDb.Id,
                    FullName = employeeInDb.FullName,
                    JobTitle = employeeInDb.JobTitle,
                    Email = employeeInDb.Email,
                    Phone = employeeInDb.Phone,
                    ImageUrl = employeeInDb.ImageUrl,
                };

                return Result<ProfileDataDTO>.Success(profile);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<ProfileDataDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<ProfileDataDTO>> GetProfileAsync(int id)
        {
            try
            {
                var employeeInDb = await _db.Employees.Where(m => m.Id == id).SingleOrDefaultAsync();

                if (employeeInDb == null)
                {
                    return Result<ProfileDataDTO>.Failure("Profile not found!");
                }

                var dto = new ProfileDataDTO()
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

                return Result<ProfileDataDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<ProfileDataDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<ProfileDataDTO>> SaveProfileAsync(ProfileDataDTO dto)
        {
            try
            {
                var employeeInDb = await _db.Employees.Where(m => m.Id == dto.Id).SingleOrDefaultAsync();

                if (employeeInDb == null)
                {
                    return Result<ProfileDataDTO>.Failure("Employee not found");
                }

                employeeInDb.FullName = dto.FullName;
                employeeInDb.JobTitle = dto.JobTitle;
                employeeInDb.Phone = dto.Phone;
                employeeInDb.CardNumber = dto.CardNumber;

                await _db.SaveChangesAsync();
                await _func.LogNewsFeedAsync(await _func.GetEmployeeNameAsync() + " updated his profile", "Users", "Home", "Profile", employeeInDb.Id);

                return Result<ProfileDataDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<ProfileDataDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<bool>> ResetPasswordAsync(string emailAddress, string callbackUrl)
        {
            try
            {
                string shortmessage = "Password reset instructions have been received for your account, " +
                    "click the button below to follow the instructions!";

                string message = _func.FormatMessage("Reset Password", shortmessage,
                    emailAddress, HtmlEncoder.Default.Encode(callbackUrl), "Reset Password");

                await _emailSender.SendEmailAsync(
                    emailAddress,
                    "Reset Password",
                    message);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<bool>.Failure(ex.Message);
            }
        }
        public async Task<Result<bool>> UploadPictureAsync(int employeeId, IFormFile? file, string webRootPath)
        {
            if (file == null || file.Length == 0)
            {
                return Result<bool>.Failure("Please select a file to upload!");
            }

            string fileName = employeeId.ToString() + Path.GetExtension(file.FileName);
            string facesPath = Path.Combine(webRootPath, @"assets\images\faces");

            try
            {
                // 1. Save the uploaded file temporarily
                var originalFilePath = Path.Combine(facesPath, "original_" + fileName);
                using (var fileStream = new FileStream(originalFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // 2. Crop the face using DNN
                var prototxtPath = Path.Combine(webRootPath, "DnnModels", "deploy.prototxt.txt");
                var caffeModelPath = Path.Combine(webRootPath, "DnnModels", "res10_300x300_ssd_iter_140000.caffemodel");
                var faceCropper = new DnnFaceCropper(prototxtPath, caffeModelPath);

                var croppedFilePath = Path.Combine(facesPath, fileName); // Final profile picture path
                var result = faceCropper.CropFace(originalFilePath, croppedFilePath);

                if (result != "Face cropped successfully.")
                {
                    // If no face detected, optionally fall back to the full original image
                    System.IO.File.Copy(originalFilePath, croppedFilePath, true);
                }

                // 3. Delete the original uploaded file to save space
                if (System.IO.File.Exists(originalFilePath))
                {
                    System.IO.File.Delete(originalFilePath);
                }

                // 4. Update employee's ImageUrl
                var employee = await _db.Employees.FirstOrDefaultAsync(m => m.Id == employeeId);
                if (employee != null)
                {
                    employee.ImageUrl = "/assets/images/faces/" + fileName;
                    await _db.SaveChangesAsync();

                    // Log the action in the news feed
                    string employeeName = await _func.GetEmployeeNameAsync();
                    await _func.LogNewsFeedAsync($"{employeeName} uploaded a profile picture", "Users", "Home", "Profile", employee.Id);
                }
                else
                {
                    return Result<bool>.Failure("Employee not found.");
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<bool>.Failure($"An error occurred while uploading the file: {ex.Message}");
            }
        }


        public async Task<Result<GlobalSearchDTO>> SearchTransferOrAssetAsync(string barcode)
        {
            try
            {
                // Search for transfer by barcode
                var transfer = await _db.Transfers.FirstOrDefaultAsync(m => m.TransferNumber == barcode);
                if (transfer != null)
                {
                    return Result<GlobalSearchDTO>.Success(new GlobalSearchDTO { TransferId = transfer.Id });
                }

                // Search for asset if transfer is not found
                var asset = await _func.AssetGlobalSearchAsync(barcode.Trim());
                GlobalSearchDTO dto = new GlobalSearchDTO();

                if (asset != null)
                {
                    var assetPicture = await _db.AssetPictures.FirstOrDefaultAsync(m => m.AssetId == asset.Id);
                    dto.Asset = asset;
                    dto.AssetPicture = assetPicture == null ? new Models.AssetPicture() : assetPicture;
                }

                return Result<GlobalSearchDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<GlobalSearchDTO>.Failure("An error occurred while searching for the transfer or asset.");
            }
        }
        public async Task<Result<NotificationRedirectorDTO>> HandleNotificationAsync(int notificationId)
        {
            try
            {
                var notification = await _db.Notifications
                    .Where(m => m.Id == notificationId)
                    .Include(m => m.NotificationSection)
                    .FirstOrDefaultAsync();

                if (notification == null)
                {
                    return Result<NotificationRedirectorDTO>.Failure("Notification not found.");
                }

                notification.IsViewed = true;
                await _db.SaveChangesAsync();

                var dto = new NotificationRedirectorDTO
                {
                    Action = notification.NotificationSection.action,
                    Controller = notification.NotificationSection.controller,
                    Area = notification.NotificationSection.area,
                    TargetRecordId = notification.TargetRecordId
                };

                return Result<NotificationRedirectorDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<NotificationRedirectorDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<List<NotificationDTO>>> GetAllNotificationsAsync(int id) {
            try
            {
                var dto = await _db.Notifications
                .Where(m => m.EmployeeTo == _employeeId)
                .Include(m => m.NotificationSection)
                .OrderByDescending(m => m.DateTime)
                .Select(notification => new NotificationDTO
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
                foreach (var notification in dto)
                {
                    notification.ImageUrl = await _func.GetProfileImageAsync(notification.EmployeeFrom);
                }

                return Result<List<NotificationDTO>>.Success(dto);

            }
            catch (Exception ex) {
                _func.LogException(_logger,ex);
                return Result<List<NotificationDTO>>.Failure(ex.Message);
            }
        }
        public async Task<Result<bool>> ClearAllNotificationsAsync() {
            try
            {
                var notifications = await _db.Notifications.Where(m => m.EmployeeTo == _employeeId).ToListAsync();
                _db.Notifications.RemoveRange(notifications);
                await _db.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex) {
                _func.LogException(_logger, ex);
                return Result<bool>.Failure(ex.Message);
            }
        }
        public async Task<Result<List<NewsFeed>>> GetListOfNewsfeedAsync() {
            try
            {
                var newsFeed = await _db.NewsFeed.OrderByDescending(m => m.TimeStamp).ToListAsync();
                return Result<List<NewsFeed>>.Success(newsFeed);
            }
            catch (Exception ex) {
                _func.LogException(_logger, ex);
                return Result<List<NewsFeed>>.Failure(ex.Message);
            }
        }

        //Private functions
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
