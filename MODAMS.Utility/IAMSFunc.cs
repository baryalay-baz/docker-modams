using Microsoft.AspNetCore.Identity;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;

namespace MODAMS.Utility
{
    public interface IAMSFunc
    {
        public int GetEmployeeId();
        public int GetEmployeeIdByEmail(string email);
        public string GetEmployeeName();
        public string GetEmployeeName(int nEmloyeeId);
        public string GetEmployeeEmail();
        public string GetDepartmentName(int nEmployeeId);
        public string GetRoleName(int nEmployeeId);
        public int GetSupervisorId(int nEmployeeId);
        public string GetSupervisorName(int nEmployeeId);
        public bool IsUserActive(string emailAddress);
        public dtoRedirection GetRedirectionObject();
        public int GetStoreId(int assetId);
        public string GetStoreName(int storeId);
        public int GetStoreOwnerId(int storeId);
        public decimal GetDepreciatedCost(int nAssetId);
        public decimal GetDepreciatedCostByStore(int storeId);

        public string FormatMessage(string title, string message, string email, string returnUrl, string btntext);

    }
}
