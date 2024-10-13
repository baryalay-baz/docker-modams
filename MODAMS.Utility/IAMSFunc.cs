using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;

namespace MODAMS.Utility
{
    public interface IAMSFunc
    {
        public int GetEmployeeId();
        public Task<int> GetEmployeeIdAsync();
        public Task<int> GetEmployeeIdByEmailAsync(string email);
        public Task<string> GetEmployeeNameAsync();
        public Task<string> GetEmployeeNameAsync(int nEmloyeeId);
        public Task<string> GetEmployeeEmailAsync();
        public Task<bool> IsInRoleAsync(string sRole, string email);
        public Task<string> GetRoleNameAsync(string email);
        public Task<int> GetDepartmentIdAsync(int nEmployeeId);
        public Task<int> GetDepartmentHeadAsync(int departmentId);
        public Task<List<Employee>> GetDepartmentMembersAsync(int nDepartmentId);
        public Task NotifyDepartmentAsync(int departmentId, Notification notification);
        public Task NotifyUserAsync(Notification notification);
        public Task NotifyUsersInRoleAsync(Notification notification, string role);
        public Task<int> IsEmailRegisteredAsync(string sEmail);
        public Task<string> GetDepartmentNameAsync(int nEmployeeId);
        public Task<dtoRedirection> GetRedirectionObjectAsync();
        public Task<string> GetDepartmentNameByIdAsync(int nDepartmentId);
        public Task<string> GetRoleNameAsync(int nEmployeeId);
        public int GetSupervisorId(int nEmployeeId);
        public Task<int> GetSupervisorIdAsync(int nEmployeeId);
        public Task<string> GetSupervisorNameAsync(int nEmployeeId);
        public Task<bool> IsUserActiveAsync(string emailAddress);
        public Task<int> GetStoreIdByAssetIdAsync(int assetId);
        public Task<int> GetStoreIdByEmployeeIdAsync(int employeeId);
        public Task<int> GetStoreIdByDepartmentIdAsync(int departmentId);
        public Task<string> GetStoreNameByStoreIdAsync(int storeId);
        public decimal GetDepreciatedCost(int nAssetId);
        public Task<decimal> GetDepreciatedCostAsync(int nAssetId);
        public Task<decimal> GetDepreciatedCostByStoreIdAsync(int storeId);
        public Task<string> GetProfileImageAsync(int employeeId);
        public Task<int> GetStoreOwnerIdAsync(int storeId);
        public Task<List<vwStore>> GetStoresByEmployeeIdAsync(int employeeId);
        public Task<string> GetEmployeeNameByIdAsync(int employeeId);
        public string FormatMessage(string title, string message, string email, string returnUrl, string btntext);
        public string GetBGColor(int counter);
        public Task<string> GetStoreOwnerInfoAsync(int storeId);
        public Task LogNewsFeedAsync(string description, string area, string controller, string action, int sourceRecordId);
        public Task<string> GetAssetNameAsync(int assetId);
        public Task RecordLoginAsync(string userId);
        public Task<int> GetEmployeeIdByUserIdAsync(string userId);
        public Task<Asset> AssetGlobalSearchAsync(string search);
        public Task<List<vwCategoryAsset>> GetvwCategoryAssetsAsync();

        public void LogException(ILogger logger, Exception ex);

    }
}
