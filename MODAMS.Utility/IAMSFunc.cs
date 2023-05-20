using Microsoft.AspNetCore.Identity;
using MODAMS.Models;
using MODAMS.Models.ViewModels;

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
        public string GetSuperpervisorName(int nEmployeeId);

		public dtoRedirection GetRedirectionObject();
    }
}
