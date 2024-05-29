using Microsoft.AspNetCore.Identity;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;

namespace MODAMS.Utility
{
    public interface IAMSFunc
    {
        public int GetEmployeeId();
        public Task<int> GetEmployeeIdByEmail(string email);
        public Task<string> GetEmployeeName();
        public Task<string> GetEmployeeName(int nEmloyeeId);
        public string GetEmployeeEmail();
        public Task<int> GetDepartmentId(int nEmployeeId);
        public int GetDepartmentHead(int nDepartmentId);
        public List<Employee> GetDepartmentMembers(int nDepartmentId);
        public Task<string> GetDepartmentName(int nEmployeeId);
        public string GetDepartmentNameById(int nDepartmentId);
        public string GetRoleName(int nEmployeeId);
        public int GetSupervisorId(int nEmployeeId);
        public string GetSupervisorName(int nEmployeeId);
        public bool IsUserActive(string emailAddress);
        public Task<dtoRedirection> GetRedirectionObject();
        public int GetStoreIdByAssetId(int assetId);
        public int GetStoreIdByEmployeeId(int employeeId);
        public int GetStoreIdByDepartmentId(int departmentId);
        public string GetStoreNameByStoreId(int storeId);
        public Task<List<vwStore>> GetStoresByEmployeeId(int employeeId);
        public int GetStoreOwnerId(int storeId);
        public string GetEmployeeNameById(int employeeId);
        public decimal GetDepreciatedCost(int nAssetId);
        public decimal GetDepreciatedCostByStoreId(int storeId);
        public string FormatMessage(string title, string message, string email, string returnUrl, string btntext);
        public void NotifyDepartment(int departmentId, Notification notification);
        public string GetProfileImage(int employeeId);
        public string GetBGColor(int counter);
        public string GetStoreOwnerInfo(int storeId);
        public void LogNewsFeed(string description, string area, string controller, string action, int sourceRecordId);
        public string GetAssetName(int assetId);
        public Task RecordLogin(string userId);
        public Task<int> GetEmployeeIdByUserId(string userId);
        public Task<Asset> AssetGlobalSearch(string search);

    }
}
