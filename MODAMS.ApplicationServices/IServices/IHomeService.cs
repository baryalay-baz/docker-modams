using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;


namespace MODAMS.ApplicationServices.IServices
{
    public interface IHomeService
    {
        Task<Result<DashboardDTO>> GetIndexAsync();
        Task<Result<List<NotificationDTO>>> GetNotificationsAsync();
        Task<Result<ProfileDataDTO>> GetProfileDataAsync();
        Task<Result<ProfileDataDTO>> GetProfileAsync(int id);
        Task<Result<ProfileDataDTO>> SaveProfileAsync(ProfileDataDTO dto);
        Task<Result<bool>> ResetPasswordAsync(string emailAddress, string callbackUrl);
        Task<Result<bool>> UploadPictureAsync(int employeeId, IFormFile? file, string webRootPath);
        Task<Result<GlobalSearchDTO>> SearchTransferOrAssetAsync(string barcode);
        Task<Result<NotificationRedirectorDTO>> HandleNotificationAsync(int notificationId);
        Task<Result<List<NotificationDTO>>> GetAllNotificationsAsync(int id);
        Task<Result<bool>> ClearAllNotificationsAsync();
        Task<Result<List<NewsFeed>>> GetListOfNewsfeedAsync();
    }
}
