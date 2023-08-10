using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using System.Text.Encodings.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Hosting;
using MODAMS.Models.ViewModels.Dto;

namespace MODAMS.Utility
{
    public class AMSFunc : IAMSFunc
    {
        public readonly ApplicationDbContext _db;
        public readonly UserManager<IdentityUser> _userManager;
        public readonly IHttpContextAccessor _contextAccessor;
        public readonly LinkGenerator _linkGenerator;
        public AMSFunc(ApplicationDbContext db, UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor, LinkGenerator linkGenerator)
        {
            _db = db;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
            _linkGenerator = linkGenerator;
        }

        public int GetEmployeeId()
        {
            var ctx = _contextAccessor.HttpContext;
            var user = _userManager.GetUserId(ctx.User);

            int nEmployeeId = _db.ApplicationUsers.Where(m => m.Id == user).Select(m => m.EmployeeId).FirstOrDefault();

            return nEmployeeId;
        }
        public int GetEmployeeIdByEmail(string email)
        {
            int nEmployeeId = _db.Employees.Where(m => m.Email == email).Select(m => m.Id).FirstOrDefault();
            return nEmployeeId;
        }
        public string GetEmployeeName()
        {
            int nEmployeeId = GetEmployeeId();



            string? sEmployeeName = _db.Employees
             .Where(m => m.Id == nEmployeeId)
             .Select(m => m.FullName)
             .FirstOrDefault();
            if (sEmployeeName == null)
                sEmployeeName = String.Empty;

            return sEmployeeName;
        }
        public string GetEmployeeName(int employeeId)
        {
            var employeeName = _db.Employees.Where(m => m.Id == employeeId).Select(m => m.FullName).FirstOrDefault();
            if (employeeName != null) {
                return employeeName;
            }
            return "";
        }
        public string GetEmployeeEmail()
        {
            int nEmployeeId = GetEmployeeId();

            string? sEmployeeEmail = _db.Employees
             .Where(m => m.Id == nEmployeeId)
             .Select(m => m.Email)
             .FirstOrDefault();
            if (sEmployeeEmail == null)
                sEmployeeEmail = String.Empty;

            return sEmployeeEmail;
        }
        public bool IsInRole(string sRole, string email)
        {
            bool blnResult = false;
            int nEmployeeId = GetEmployeeIdByEmail(email);

            var rec = _db.vwEmployees.Where(m => m.Id == nEmployeeId && m.RoleName == sRole).FirstOrDefault();
            if (rec != null)
            {
                blnResult = true;
            }
            return blnResult;
        }
        public string GetRoleName(string email)
        {
            string sResult = "-";
            int nEmployeeId = GetEmployeeIdByEmail(email);

            var rec = _db.vwEmployees.Where(m => m.Id == nEmployeeId).Select(m => m.RoleName).FirstOrDefault();
            if (rec != null)
            {
                sResult = rec;
            }
            return sResult;
        }
        public int GetDepartmentHead(int departmentId) {
            int employeeId = 0;
            var departmentHead = _db.DepartmentHeads
                .Where(m => m.DepartmentId == departmentId && m.IsActive)
                .FirstOrDefault();

            if (departmentHead != null)
            {
                employeeId = departmentHead.EmployeeId;
            }
            return employeeId;
        }
        public List<Employee> GetDepartmentMembers(int departmentId) {
            int employeeId = GetDepartmentHead(departmentId);
            List<Employee> employees = _db.Employees.Where(m=>m.SupervisorEmployeeId==employeeId).ToList();

            var employee = _db.Employees.Where(m => m.Id == employeeId).FirstOrDefault();
            if(employee!=null)
                employees.Add(employee);

            return employees;
        }
        public void NotifyDepartment(int departmentId, Notification notification)
        {
            var employees = GetDepartmentMembers(departmentId);
            var ids = employees.Select(m => m.Id).ToList();
            int[] empArray = ids.ToArray();
            Notify(empArray, notification);
        }
        public void NotifyUser(Notification notification)
        {
            int[] empArray = { notification.EmployeeTo };
            Notify(empArray, notification);
        }
        public void NotifyUsersInRole(Notification notification, string role)
        {
            int[] ids = _db.vwEmployees.Where(m => m.RoleName == role)
             .Select(m => m.Id).ToArray();

            if (ids.Length > 0)
            {
                Notify(ids, notification);
            }
        }
        public int IsEmailRegistered(string sEmail)
        {
            string sEmployeeId = "0";
            var employeeInDb = _db.Employees.Where(m => m.Email == sEmail).FirstOrDefault();
            if (employeeInDb != null)
            {
                sEmployeeId = employeeInDb.Id.ToString();
            }
            return Convert.ToInt32(sEmployeeId);
        }
        public dtoRedirection GetRedirectionObject()
        {

            string sRoleName = GetRoleName(GetEmployeeEmail());
            dtoRedirection dto = new dtoRedirection();
            if (sRoleName == SD.Role_User)
            {
                dto = new dtoRedirection("Users", "Home", "Index");
            }
            else if (sRoleName == SD.Role_StoreOwner)
            {
                dto = new dtoRedirection("Admin", "Home", "Index");
            }
            else if (sRoleName == SD.Role_Administrator)
            {
                dto = new dtoRedirection("Security", "Home", "Index");
            }
            else //Senior Management
            {
                dto = new dtoRedirection("Driver", "Home", "Index");
            }

            return dto;
        }
        public int GetDepartmentId(int nEmployeeId) {
            int departmentId = 0;
            var department = _db.Departments.Where(m=>m.EmployeeId == nEmployeeId).FirstOrDefault();
            if (department != null)
            {
                departmentId = department.Id;
            }
            return departmentId;
        }
        public string GetDepartmentName(int nEmployeeId)
        {
            string sResult = "Department not available!";
            int nDepartmentId = GetDepartmentId(nEmployeeId);
            var department = _db.Departments.Where(m=>m.EmployeeId==nEmployeeId).FirstOrDefault();
            if(department != null)
            {
                sResult = department.Name;
            }
            return sResult;
        }
        public string GetRoleName(int nEmployeeId)
        {
            string? rolename = _db.vwEmployees.Where(m => m.Id == nEmployeeId).Select(m => m.RoleName).FirstOrDefault();
            if (rolename == null)
            {
                rolename = "No role assigned";
            }
            return rolename;
        }
        public int GetSupervisorId(int nEmployeeId)
        {
            var employee = _db.Employees.Where(m => m.Id == nEmployeeId).FirstOrDefault();
            if (employee == null)
            {
                return 0;
            }
            else
            {
                return employee.SupervisorEmployeeId;
            }
        }
        public string GetSupervisorName(int nEmployeeId)
        {
            var employee = _db.vwEmployees.Where(m => m.Id == nEmployeeId).FirstOrDefault();
            string? supervisorName = "Supervisor not available!";
            if (employee != null)
            {
                var rec = _db.Employees.Where(m => m.Id == employee.SupervisorEmployeeId).SingleOrDefault();
                if (rec != null)
                {
                    supervisorName = rec.FullName;
                }
            }
            return supervisorName;
        }
        public int GetStoreIdByAssetId(int assetId)
        {
            int storeId = 0;
            if (assetId > 0)
            {
                var asset = _db.Assets.Where(m => m.Id == assetId).FirstOrDefault();
                if (asset != null)
                {
                    storeId = asset.StoreId;
                }
            }
            return storeId;
        }
        public int GetStoreIdByDepartmentId(int departmentId) {
            int storeId = 0;
            var store = _db.Stores.Where(m=>m.DepartmentId == departmentId).FirstOrDefault();
            if (store != null)
            {
                storeId = store.Id;
            }
            return storeId;
        }
        public string GetStoreNameByStoreId(int storeId)
        {
            string storeName = "";
            if (storeId > 0)
            {
                var store = _db.Stores.Where(m => m.Id == storeId).Select(m => m.Name).FirstOrDefault();
                if (store != null)
                    storeName = store.ToString();
            }
            return storeName;
        }
        public decimal GetDepreciatedCost(int nAssetId)
        {
            //(Cost / LifeSpan_months) * (LifeSpan_months - Age)
            decimal depreciatedCost = 0;

            if (nAssetId > 0)
            {
                var asset = _db.Assets.Where(m => m.Id == nAssetId).Include(m => m.SubCategory).FirstOrDefault();
                if (asset != null)
                {
                    int nLifeSpan = asset.SubCategory.LifeSpan;
                    decimal cost = asset.Cost;

                    if (asset.RecieptDate != null)
                    {
                        DateTimeOffset date1 = (DateTimeOffset)asset.RecieptDate;
                        DateTimeOffset date2 = DateTime.Now;

                        //find difference between two dates in months
                        int age = (date2.Year - date1.Year) * 12 + date2.Month - date1.Month;

                        depreciatedCost = (cost / nLifeSpan) * (nLifeSpan - age);
                    }
                }
            }

            if (depreciatedCost < 0)
                depreciatedCost = 0;

            return depreciatedCost;
        }

        //Private methods
        private void Notify(int[] arrEmpIds, Notification notification)
        {
            foreach (int id in arrEmpIds)
            {
                var newNotification = new Notification()
                {
                    DateTime = DateTime.Now,
                    EmployeeFrom = notification.EmployeeFrom,
                    EmployeeTo = notification.EmployeeTo,
                    IsViewed = false,
                    Message = notification.Message,
                    Subject = notification.Subject,
                    TargetRecordId = notification.TargetRecordId,
                    TargetSectionId = notification.TargetSectionId
                };
                newNotification.EmployeeTo = id;
                _db.Notifications.Add(newNotification);
                _db.SaveChanges();

                SendEmail(id, notification.Subject, notification.Message, notification.TargetRecordId, notification.TargetSectionId);
            }
        }
        private async void SendEmail(int empId, string subject, string message, int recordId, int sectionId)
        {
            EmailSender sender = new EmailSender();

            var emp = _db.Employees.Where(m => m.Id == empId).FirstOrDefault();
            string sEmail = "";
            string sFullName = "";

            if (emp != null)
            {
                sEmail = emp.Email;
                sFullName = emp.FullName;
            }

            var returnUrl = GenerateUrl(sectionId, recordId);
            string sEmailMessage = FormatMessage(subject, message, sFullName, returnUrl,"click here");

            try
            {
                await sender.SendEmailAsync(
                sEmail,
                subject,
                sEmailMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public bool IsUserActive(string emailAddress)
        {
            var blnResult = false;
            var employee = _db.Employees.Where(m => m.Email == emailAddress).FirstOrDefault();
            if (employee != null)
            {
                if (employee.IsActive)
                {
                    blnResult = true;
                }
            }
            return blnResult;
        }

        private string GenerateUrl(int targetSectionId, int targetRecordId)
        {
            string? callbackUrl = "";

            var section = _db.NotificationSections.Where(m => m.Id == targetSectionId).FirstOrDefault();

            if (section != null)
            {
                string sAction = section.action;
                string sController = section.controller;
                string sArea = section.area;


                callbackUrl = _linkGenerator
                    .GetUriByAction(_contextAccessor.HttpContext, sAction, sController,
                    values: new { area = sArea, id = targetRecordId });
            }
            else
            {
                callbackUrl = "~/";
            }

            return HtmlEncoder.Default.Encode(callbackUrl);
        }

        public string FormatMessage(string title, string message, string email, string returnUrl, string btntext)
        {
            string emailMessage = "";
            string initials = email.Substring(0, 1).ToUpper();
            string src = SD.WebAddress;
            
            emailMessage = "<div class=\"container\"> <div class=\"row\"><div class=\"col-md-12\"><br><br /></div> </div> <div class=\"row\"><div class=\"col-lg-12\"><table class=\"body-wrap\" style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; width: 100%; background-color: transparent; margin: 0;\" bgcolor=\"transparent\"> <tr style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; margin: 0;\"><td style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; vertical-align: top; margin: 0;\" valign=\"top\"></td><td class=\"container\" width=\"600\" style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; vertical-align: top; display: block !important; max-width: 600px !important; clear: both !important; margin: 0 auto;\" valign=\"top\"><div class=\"content\" style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; max-width: 600px; display: block; margin: 0 auto; padding: 20px;\"> <table class=\"main\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" itemprop=\"action\" itemscope itemtype=\"http://schema.org/ConfirmAction\" style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; border-radius: 3px; background-color: transparent; margin: 0; border: 1px dashed #4d79f6;\" bgcolor=\"#fff\"><tr style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; margin: 0;\"><td class=\"content-wrap\" style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; vertical-align: top; margin: 0; padding: 20px;\" valign=\"top\"> <meta itemprop=\"name\" content=\"Confirm Email\" style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; margin: 0;\" /> <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; margin: 0;\">" +
                 "<tr><img src=\"" + src + "/assets/images/brand/FGS_Small.png\" alt=\"\" style=\"margin-left: auto; margin-right: auto; display:block; margin-bottom: 10px; height: 100px;\"></td></tr><tr style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; margin: 0;\"><td class=\"content-block\" style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; color: #4e5e69; font-size: 24px; font-weight: 700; text-align: center; vertical-align: top; margin: 0; padding: 0 0 10px;\" valign=\"top\"> <hr style=\"border-color: #2541f7; border-style:dashed; border-width:0.5px;\" /></td></tr><tr style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; margin: 0;\"><td class=\"content-block\" style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; color: #3f5db3; font-size: 18px; vertical-align: top; margin: 0; padding: 10px 10px;\" valign=\"top\" align=\"center\">" +
                title +
                "</td></tr><tr style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; margin: 0;\"><td class=\"content-block\" style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; vertical-align: top; margin: 0; padding: 10px 10px;\" valign=\"top\" align=\"left\"><br />" +
                "Dear " + email + ", <br /><br />" + message +
                "<br /><br /><span style=\"font-size: 10px;\">" +
                "Note: This email is generated automatically, please do not reply" +
                "<br><br></span></td></tr><tr style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; margin: 0;\"><td class=\"content-block\" itemprop=\"handler\" itemscope itemtype=\"http://schema.org/HttpActionHandler\" style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; vertical-align: top; margin: 0; padding: 10px 10px;\" valign=\"top\">" +
                "<a href=\"" + returnUrl + "\" class=\"btn btn-primary\" style=\"color:white; background-color:#2541f7; font-size: 14px; text-decoration: none; line-height: 2em; font-weight: bold; text-align: center; cursor: pointer; display: block; border-radius: 5px; text-transform: capitalize; border: none; padding: 10px 20px;\">" + btntext + " </a></td></tr><tr style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; margin: 0;\"><td class=\"content-block\" style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; padding-top: 5px; vertical-align: top; margin: 0; text-align: center;\" valign=\"top\"> <br /> <div><img src=\"" + src + "/assets/images/brand/ams_small.png\" /> &nbsp;|&nbsp;Asset Management System </div></td></tr> </table></td></tr> </table><!--end table--></div><!--end content--></td><td style=\"font-family: 'Helvetica Neue',Helvetica,Arial,sans-serif; box-sizing: border-box; font-size: 14px; vertical-align: top; margin: 0;\" valign=\"top\"></td> </tr></table><!--end table--></div><!--end col--> </div><!--end row--></div>";

            return emailMessage;
        }

        public decimal GetDepreciatedCostByStoreId(int storeId)
        {
            var assetList = _db.Assets.Where(m => m.StoreId == storeId)
                .Select(m => new { m.Id }).ToList();

            decimal totalCost = 0;

            foreach (var asset in assetList)
            {
                totalCost += GetDepreciatedCost(asset.Id);
            }
            return Math.Round(totalCost, 0);
        }

        public int GetStoreOwnerId(int storeId)
        {
            int departmentId = 0;
            int storeOwnerId = 0;

            var store = _db.Stores.Where(m => m.Id == storeId).FirstOrDefault();
            if (store != null)
            {
                departmentId = store.DepartmentId;
                var departmentHead = _db.DepartmentHeads
                    .Where(m => m.DepartmentId == departmentId && m.IsActive == true).FirstOrDefault();
                if (departmentHead != null)
                {
                    storeOwnerId = departmentHead.EmployeeId;
                }
            }
            return storeOwnerId;
        }

        public int GetStoreIdByEmployeeId(int employeeId)
        {
            int storeId = 0;
            var department = _db.Departments.Where(m=>m.EmployeeId== employeeId).FirstOrDefault();
            if (department != null)
            {
                var store = _db.Stores.Where(m => m.DepartmentId == department.Id).FirstOrDefault();
                if(store != null)
                {
                    storeId = store.Id; 
                }
            }
            return storeId;
        }

        public string GetEmployeeNameById(int employeeId)
        {
            string EmployeeName = "Not found!";
            var employee = _db.Employees.Where(m=>m.Id== employeeId).FirstOrDefault();
            if (employee != null)
            {
                EmployeeName = employee.FullName;
            }
            return EmployeeName;
        }

        
    }
}
