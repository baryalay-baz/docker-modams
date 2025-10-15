using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using System.Text.Encodings.Web;
using Microsoft.EntityFrameworkCore;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Models.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;


namespace MODAMS.Utility
{
    public class AMSFunc : IAMSFunc
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly ILogger<AMSFunc> _logger;
        private readonly IEmailSender _emailSender;
        private readonly bool _isSomali;
        public AMSFunc(ApplicationDbContext db, UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor,
            LinkGenerator linkGenerator, ILogger<AMSFunc> logger, IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
            _linkGenerator = linkGenerator;
            _logger = logger;
            _emailSender = emailSender;
            _isSomali = CultureInfo.CurrentUICulture.Name == "so";
        }
        //public int GetEmployeeId()
        //{
        //    int EmployeeId = 0;
        //    // Get the current claims principal
        //    var user = _contextAccessor.HttpContext.User;

        //    // Find the user's ID claim
        //    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        //    if (userIdClaim != null)
        //        EmployeeId = _db.ApplicationUsers.Where(m => m.Id == userIdClaim.Value).Select(m => m.EmployeeId).FirstOrDefault();

        //    return EmployeeId;
        //}

        public int GetEmployeeId()
        {
            // Get the current user
            var user = _userManager.GetUserAsync(_contextAccessor.HttpContext.User).Result as ApplicationUser;

            // Return the user's EmployeeId if user is found
            return user?.EmployeeId ?? 0;
        }
        public async Task<int> GetEmployeeIdAsync()
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User) as ApplicationUser;

            // Return the user's EmployeeId if user is found
            return user?.EmployeeId ?? 0;
        }
        public async Task<int> GetEmployeeIdByUserIdAsync(string userId)
        {
            int EmployeeId = 0;

            EmployeeId = await _db.ApplicationUsers.Where(m => m.Id == userId).Select(m => m.EmployeeId).FirstOrDefaultAsync();

            return EmployeeId == 0 ? 0 : EmployeeId;
        }
        public async Task<int> GetEmployeeIdByEmailAsync(string email)
        {
            int nEmployeeId = await _db.Employees.Where(m => m.Email == email).Select(m => m.Id).FirstOrDefaultAsync();
            return nEmployeeId;
        }
        public async Task<string> GetEmployeeNameAsync()
        {
            int nEmployeeId = GetEmployeeId();



            string? sEmployeeName = await _db.Employees
             .Where(m => m.Id == nEmployeeId)
             .Select(m => m.FullName)
             .FirstOrDefaultAsync();

            //if sEmployee==null it will return string.Empty else sEmployeeName
            return sEmployeeName ?? string.Empty;

        }
        public async Task<string> GetEmployeeNameAsync(int employeeId)
        {
            var employeeName = await _db.Employees.Where(m => m.Id == employeeId).Select(m => m.FullName).FirstOrDefaultAsync();
            if (employeeName != null)
            {
                return employeeName;
            }
            return "";
        }
        public async Task<string> GetEmployeeEmailAsync()
        {
            int nEmployeeId = await GetEmployeeIdAsync();

            string? sEmployeeEmail = await _db.Employees
             .Where(m => m.Id == nEmployeeId)
             .Select(m => m.Email)
             .FirstOrDefaultAsync();
            if (sEmployeeEmail == null)
                sEmployeeEmail = String.Empty;

            return sEmployeeEmail;
        }
        public async Task<bool> IsInRoleAsync(string sRole, string email)
        {
            bool blnResult = false;
            int nEmployeeId = await GetEmployeeIdByEmailAsync(email);

            var rec = _db.vwEmployees.Where(m => m.Id == nEmployeeId && m.RoleName == sRole).FirstOrDefault();
            if (rec != null)
            {
                blnResult = true;
            }
            return blnResult;
        }
        public async Task<bool> IsInRoleAsync(string sRole, int employeeId)
        {
            bool blnResult = false;

            var rec = await _db.vwEmployees.Where(m => m.Id == employeeId && m.RoleName == sRole).FirstOrDefaultAsync();
            if (rec != null)
            {
                blnResult = true;
            }
            return blnResult;
        }
        public async Task<string> GetRoleNameAsync(string email)
        {
            string sResult = "-";
            int nEmployeeId = await GetEmployeeIdByEmailAsync(email);

            var rec = await _db.vwEmployees.Where(m => m.Id == nEmployeeId).Select(m => m.RoleName).FirstOrDefaultAsync();
            if (rec != null)
            {
                sResult = rec;
            }
            return sResult;
        }
        public async Task<int> GetDepartmentHeadAsync(int departmentId)
        {
            int employeeId = 0;
            var departmentHead = await _db.DepartmentHeads
                .Where(m => m.DepartmentId == departmentId && m.IsActive)
                .FirstOrDefaultAsync();

            if (departmentHead != null)
            {
                employeeId = departmentHead.EmployeeId;
            }
            return employeeId;
        }
        public async Task<List<Employee>> GetDepartmentMembersAsync(int departmentId)
        {
            int employeeId = await GetDepartmentHeadAsync(departmentId);
            List<Employee> employees = _db.Employees.Where(m => m.SupervisorEmployeeId == employeeId).ToList();

            var employee = _db.Employees.Where(m => m.Id == employeeId).FirstOrDefault();
            if (employee != null)
                employees.Add(employee);

            return employees;
        }
        public async Task NotifyDepartmentAsync(int departmentId, Notification notification)
        {
            var employees = await GetDepartmentMembersAsync(departmentId);
            var ids = employees.Select(m => m.Id).ToList();
            int[] empArray = ids.ToArray();
            await NotifyAsync(empArray, notification);
        }
        public async Task NotifyUserAsync(Notification notification)
        {
            int[] empArray = { notification.EmployeeTo };
            await NotifyAsync(empArray, notification);
        }
        public async Task NotifyUsersInRoleAsync(Notification notification, string role)
        {
            int[] ids = await _db.vwEmployees.Where(m => m.RoleName == role)
             .Select(m => m.Id).ToArrayAsync();

            if (ids.Length > 0)
            {
                await NotifyAsync(ids, notification);
            }
        }
        public async Task<int> IsEmailRegisteredAsync(string sEmail)
        {
            string sEmployeeId = "0";
            var employeeInDb = await _db.Employees.Where(m => m.Email == sEmail).FirstOrDefaultAsync();
            if (employeeInDb != null)
            {
                sEmployeeId = employeeInDb.Id.ToString();
            }
            return Convert.ToInt32(sEmployeeId);
        }
        public async Task<RedirectionDTO> GetRedirectionObjectAsync()
        {
            string sRoleName = await GetRoleNameAsync(await GetEmployeeEmailAsync());
            var dto = sRoleName switch
            {
                SD.Role_User => new RedirectionDTO("Users", "Home", "Index"),
                SD.Role_StoreOwner => new RedirectionDTO("Admin", "Home", "Index"),
                SD.Role_Administrator => new RedirectionDTO("Security", "Home", "Index"),
                _ => new RedirectionDTO("Driver", "Home", "Index")
            };
            return dto;
        }
        public async Task<int> GetDepartmentIdByEmployeeIdAsync(int employeeId)
        {
            var role = await GetRoleNameAsync(employeeId);

            return role switch
            {
                SD.Role_User => await _db.StoreEmployees
                    .Where(se => se.EmployeeId == employeeId)
                    .Select(se => se.Store.DepartmentId)
                    .FirstOrDefaultAsync(),

                SD.Role_StoreOwner => await _db.Departments
                    .Where(d => d.EmployeeId == employeeId)
                    .Select(d => d.Id)
                    .FirstOrDefaultAsync(),

                _ => 0
            };
        }
        public async Task<string> GetDepartmentNameAsync(int nEmployeeId)
        {
            int nDepartmentId = await GetDepartmentIdByEmployeeIdAsync(nEmployeeId);
            return await GetDepartmentNameByIdAsync(nDepartmentId);
        }
        public async Task<string> GetDepartmentNameByIdAsync(int departmentId)
        {
            var name = await _db.Departments
                .AsNoTracking()
                .Where(d => d.Id == departmentId)
                .Select(d => _isSomali ? d.NameSo : d.Name)
                .FirstOrDefaultAsync();

            return string.IsNullOrWhiteSpace(name)
                ? (_isSomali ? "Waax lama heli karo!" : "Department not available!")
                : name;
        }
        public async Task<string> GetRoleNameAsync(int nEmployeeId)
        {
            string? rolename = await _db.vwEmployees.Where(m => m.Id == nEmployeeId).Select(m => m.RoleName).FirstOrDefaultAsync();
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
        public async Task<int> GetSupervisorIdAsync(int nEmployeeId)
        {
            var employee = await _db.Employees.Where(m => m.Id == nEmployeeId).FirstOrDefaultAsync();
            if (employee == null)
            {
                return 0;
            }
            else
            {
                return employee.SupervisorEmployeeId;
            }
        }
        public async Task<string> GetSupervisorNameAsync(int nEmployeeId)
        {
            var employee = await _db.vwEmployees.Where(m => m.Id == nEmployeeId).FirstOrDefaultAsync();
            string? supervisorName = "Supervisor not available!";
            if (employee != null)
            {
                var rec = await _db.Employees.Where(m => m.Id == employee.SupervisorEmployeeId).SingleOrDefaultAsync();
                if (rec != null)
                {
                    supervisorName = rec.FullName;
                }
            }
            return supervisorName;
        }
        public async Task<int> GetStoreIdByAssetIdAsync(int assetId)
        {
            int storeId = 0;
            if (assetId > 0)
            {
                var asset = await _db.Assets.Where(m => m.Id == assetId).FirstOrDefaultAsync();
                if (asset != null)
                {
                    storeId = asset.StoreId;
                }
            }
            return storeId;
        }
        public async Task<int> GetStoreIdByEmployeeIdAsync(int employeeId)
        {
            if (await IsInRoleAsync(SD.Role_StoreOwner, employeeId))
            {
                var storeId = await _db.Departments
                    .Where(d => d.EmployeeId == employeeId)
                    .SelectMany(d => d.Stores)
                    .Select(s => (int?)s.Id)
                    .FirstOrDefaultAsync();

                return storeId ?? 0;
            }

            if (await IsInRoleAsync(SD.Role_User, employeeId))
            {
                var storeId = await _db.StoreEmployees
                    .Where(se => se.EmployeeId == employeeId)
                    .Select(se => (int?)se.StoreId)
                    .FirstOrDefaultAsync();

                return storeId ?? 0;
            }
            return 0;
        }
        public async Task<int> GetStoreIdByDepartmentIdAsync(int departmentId)
        {
            int storeId = 0;
            var store = await _db.Stores.Where(m => m.DepartmentId == departmentId).FirstOrDefaultAsync();
            if (store != null)
            {
                storeId = store.Id;
            }
            return storeId;
        }
        public async Task<string> GetStoreNameByStoreIdAsync(int storeId)
        {
            string storeName = "";
            if (storeId > 0)
            {
                var store = await _db.Stores.Where(m => m.Id == storeId).Select(m => _isSomali ? m.NameSo : m.Name).FirstOrDefaultAsync();
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
        public async Task<decimal> GetDepreciatedCostAsync(int nAssetId)
        {
            //(Cost / LifeSpan_months) * (LifeSpan_months - Age)
            decimal depreciatedCost = 0;

            if (nAssetId > 0)
            {
                var asset = await _db.Assets.Where(m => m.Id == nAssetId).Include(m => m.SubCategory).FirstOrDefaultAsync();
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
        public async Task<decimal> GetDepreciatedCostByStoreIdAsync(int storeId)
        {
            var assetList = await _db.Assets.Where(m => m.StoreId == storeId)
                .Select(m => new { m.Id }).ToListAsync();

            decimal totalCost = 0;

            foreach (var asset in assetList)
            {
                totalCost += GetDepreciatedCost(asset.Id);
            }
            return Math.Round(totalCost, 0);
        }
        public async Task<string> GetProfileImageAsync(int employeeId)
        {
            var employee = await _db.Employees.FirstOrDefaultAsync(m => m.Id == employeeId);
            return employee?.ImageUrl ?? "";
        }
        public string FormatMessage(string title, string message, string fullNameOrEmail, string returnUrl, string btntext)
        {
            string emailMessage = string.Empty;
            string src = SD.WebAddress;

            // Generate salutation
            string salutation = _isSomali
                ? $"{fullNameOrEmail} sharafta leh,"
                : $"Dear {fullNameOrEmail},";

            string noteLine = _isSomali
                ? "Fiiro gaar ah: Email-kan si toos ah ayaa loo soo diray, fadlan ha ka jawaabin."
                : "Note: This email is generated automatically, please do not reply.";
            emailMessage = $@"
                <div class='container'>
                  <div class='row'>
                    <div class='col-md-12'><br><br /></div>
                  </div>
                  <div class='row'>
                    <div class='col-lg-12'>
                      <table class='body-wrap' style='font-family: Helvetica Neue,Helvetica,Arial,sans-serif; font-size: 14px; width: 100%; background-color: transparent; margin: 0;'>
                        <tr>
                          <td></td>
                          <td class='container' width='600' style='max-width: 600px; margin: 0 auto; display: block; clear: both;'>
                            <div class='content' style='padding: 20px;'>
                              <table class='main' width='100%' cellpadding='0' cellspacing='0' style='border-radius: 3px; border: 1px dashed #4d79f6; background-color: #fff;'>
                                <tr>
                                  <td class='content-wrap' style='padding: 20px;'>
                                    <table width='100%' cellpadding='0' cellspacing='0'>
                                      <tr>
                                        <td align='center'>
                                          <img src='{src}/assets/images/brand/FGS_Small.png' alt='FGS Logo' style='display:block; margin: 0 auto 10px; height: 100px;' />
                                        </td>
                                      </tr>
                                      <tr>
                                        <td align='center' style='font-size: 24px; font-weight: 700; color: #4e5e69;'>
                                          <hr style='border-color: #2541f7; border-style:dashed; border-width:0.5px;' />
                                        </td>
                                      </tr>
                                      <tr>
                                        <td align='center' style='font-size: 18px; color: #3f5db3; padding: 10px 10px;'>
                                          {title}
                                        </td>
                                      </tr>
                                      <tr>
                                        <td align='left' style='padding: 10px 10px; font-size: 14px;'>
                                          <br />
                                          {salutation}<br /><br />
                                          {message}
                                          <br /><br />
                                          <span style='font-size: 10px; color: #888;'>{noteLine}</span>
                                          <br><br />
                                        </td>
                                      </tr>
                                      <tr>
                                        <td align='center' style='padding: 10px 10px;'>
                                          <a href='{returnUrl}' class='btn btn-primary' style='color:white; background-color:#2541f7; font-size: 14px; text-decoration: none; font-weight: bold; border-radius: 5px; padding: 10px 20px; display: inline-block;'>{btntext}</a>
                                        </td>
                                      </tr>
                                      <tr>
                                        <td align='center' style='padding-top: 10px;'>
                                          <img src='{src}/assets/images/brand/ams_small.png' alt='AMS Footer Logo' />
                                        </td>
                                      </tr>
                                    </table>
                                  </td>
                                </tr>
                              </table>
                            </div>
                          </td>
                          <td></td>
                        </tr>
                      </table>
                    </div>
                  </div>
                </div>";

            return emailMessage;
        }
        public async Task<int> GetStoreOwnerIdAsync(int storeId)
        {
            int departmentId = 0;
            int storeOwnerId = 0;

            var store = await _db.Stores.Where(m => m.Id == storeId).FirstOrDefaultAsync();
            if (store != null)
            {
                departmentId = store.DepartmentId;
                var departmentHead = await _db.DepartmentHeads
                    .Where(m => m.DepartmentId == departmentId && m.IsActive == true).FirstOrDefaultAsync();
                if (departmentHead != null)
                {
                    storeOwnerId = departmentHead.EmployeeId;
                }
            }
            return storeOwnerId;
        }
        public async Task<List<vwStore>> GetStoresByEmployeeIdAsync(int employeeId)
        {
            var roleName = await GetRoleNameAsync(employeeId);
            List<vwStore> result;

            if (roleName == "User")
            {
                result = await _db.StoreEmployees
                    .Where(se => se.EmployeeId == employeeId)
                    .Join(_db.vwStores,
                          se => se.StoreId,
                          store => store.Id,
                          (_, store) => store)
                    .ToListAsync();

                result.ForEach(s => s.StoreType = 1);
            }
            else if (roleName == "StoreOwner")
            {
                var ownerStore = await _db.vwStores
                    .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);
                if (ownerStore == null)
                    return new List<vwStore>();

                var all = await _db.vwStores.ToListAsync();
                var finder = new StoreFinder((int)ownerStore.DepartmentId, all);
                result = finder.GetStores().ToList();

                result.ForEach(s => s.StoreType = (s.Id == ownerStore.Id) ? 1 : 0);
            }
            else
            {
                result = await _db.vwStores.ToListAsync();
            }

            return result
                .OrderByDescending(s => s.StoreType)
                .ThenByDescending(s => s.TotalCount)
                .ToList();
        }

        public async Task<string> GetEmployeeNameByIdAsync(int employeeId)
        {
            string EmployeeName = "Not found!";
            var employee = await _db.Employees.Where(m => m.Id == employeeId).FirstOrDefaultAsync();
            if (employee != null)
            {
                EmployeeName = employee.FullName;
            }
            return EmployeeName;
        }
        public string GetBGColor(int counter)
        {
            string sResult = "";
            switch (counter)
            {
                case 1:
                    sResult = "bg-primary";
                    break;
                case 2:
                    sResult = "bg-secondary";
                    break;
                case 3:
                    sResult = "bg-warning";
                    break;
                case 4:
                    sResult = "bg-info";
                    break;
                case 5:
                    sResult = "bg-danger";
                    break;
                case 6:
                    sResult = "bg-primary";
                    break;
                case 7:
                    sResult = "bg-secondary";
                    break;
                case 8:
                    sResult = "bg-warning";
                    break;
            }
            return sResult;
        }
        public async Task<string> GetStoreOwnerInfoAsync(int storeId)
        {
            string sResult = "Vacant";
            string storeOwner = "";
            string emailAddress = "";
            string imageUrl = "";
            bool blnCheck = false;

            var employeeId = await GetStoreOwnerIdAsync(storeId);
            var rec = await _db.Employees.FirstOrDefaultAsync(m => m.Id == employeeId);
            if (rec != null)
            {
                storeOwner = rec.FullName;
                emailAddress = rec.Email;
                imageUrl = rec.ImageUrl;
                blnCheck = true;
            }

            if (blnCheck)
            {
                sResult = "<div class=\"d-flex align-items-center\">" +
                    "<div class=\"me-2\"><span>" +
                    "<img style=\"min-width:30px;\" src=\"" + imageUrl + "\" alt=\"profile-user\" class=\"data-image avatar avatar-lg rounded-circle\">" +
                    "</span></div><div><h6 class=\"mb-0\">" + storeOwner + "</h6><span class=\"text-muted fs-12\">" + emailAddress + "</span>\r\n" +
                    "</div></div>";
            }
            else
            {
                sResult = "<span class=\"text-secondary\">Store is vacant</span>";
            }

            return sResult;
        }
        public async Task<bool> IsUserActiveAsync(string emailAddress)
        {
            var blnResult = false;
            var employee = await _db.Employees.Where(m => m.Email == emailAddress).FirstOrDefaultAsync();
            if (employee != null)
            {
                if (employee.IsActive)
                {
                    blnResult = true;
                }
            }
            return blnResult;
        }
        public async Task LogNewsFeedAsync(string description, string area, string controller, string action, int sourceRecordId)
        {
            NewsFeed feed = new NewsFeed()
            {
                Description = description,
                Area = area,
                Controller = controller,
                Action = action,
                SourceRecordId = sourceRecordId,
                EmployeeId = GetEmployeeId(),
                TimeStamp = DateTime.Now
            };
            await _db.NewsFeed.AddAsync(feed);
            await _db.SaveChangesAsync();
        }
        public async Task<string> GetAssetNameAsync(int assetId)
        {
            var asset = await _db.Assets.Where(m => m.Id == assetId).FirstOrDefaultAsync();

            string assetName = "";
            if (asset != null)
            {
                assetName = asset.Name.ToString();
            }
            return assetName;
        }
        public async Task RecordLoginAsync(string userId)
        {

            string ipAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            LoginHistory login = new LoginHistory()
            {
                EmployeeId = await GetEmployeeIdByUserIdAsync(userId),
                IPAddress = ipAddress,
                TimeStamp = DateTime.Now
            };
            _db.LoginHistory.Add(login);
            _db.SaveChanges();
        }
        public async Task<List<AssetSearchDTO>> AssetGlobalSearchAsync(string rawSearch, int take = 100)
        {
            if (string.IsNullOrWhiteSpace(rawSearch))
                return new();

            int vehicleCategoryId = await _db.Categories
                .Where(c => c.CategoryName == "Vehicles")
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync() ?? 0;

            var searchU = rawSearch.Trim().ToUpperInvariant();
            var likeU = $"%{searchU}%";

            var baseQ = _db.Assets
                .AsNoTracking()
                .Where(a => a.AssetStatusId != SD.Asset_Deleted)
                .Select(a => new
                {
                    A = a,
                    CatName = _isSomali ? a.SubCategory.Category.CategoryNameSo : a.SubCategory.Category.CategoryName,
                    SubName = _isSomali ? a.SubCategory.SubCategoryNameSo : a.SubCategory.SubCategoryName,
                    StoreName = _isSomali ? a.Store.NameSo : a.Store.Name
                });

            var results = await baseQ
                .Where(x =>
                    (x.A.Barcode != null && (
                        x.A.Barcode.ToUpper() == searchU)) ||

                    (x.A.SerialNo != null && (
                        x.A.SerialNo.ToUpper() == searchU ||
                        x.A.SerialNo.ToUpper().StartsWith(searchU) ||
                        EF.Functions.Like(x.A.SerialNo.ToUpper(), likeU)
                    )) ||

                    (x.A.Plate != null && (
                        x.A.Plate.ToUpper() == searchU ||
                        x.A.Plate.ToUpper().StartsWith(searchU) ||
                        EF.Functions.Like(x.A.Plate.ToUpper(), likeU)
                    )) ||

                    (x.A.Engine != null && (
                        x.A.Engine.ToUpper() == searchU ||
                        x.A.Engine.ToUpper().StartsWith(searchU) ||
                        EF.Functions.Like(x.A.Engine.ToUpper(), likeU)
                    )) ||

                    (x.A.Chasis != null && (
                        x.A.Chasis.ToUpper() == searchU ||
                        x.A.Chasis.ToUpper().StartsWith(searchU) ||
                        EF.Functions.Like(x.A.Chasis.ToUpper(), likeU)
                    )) ||

                    (x.A.Model != null && (
                        x.A.Model.ToUpper() == searchU ||
                        x.A.Model.ToUpper().StartsWith(searchU) ||
                        EF.Functions.Like(x.A.Model.ToUpper(), likeU)
                    )) ||

                    (x.A.Make != null && (
                        x.A.Make.ToUpper() == searchU ||
                        x.A.Make.ToUpper().StartsWith(searchU) ||
                        EF.Functions.Like(x.A.Make.ToUpper(), likeU)
                    )) ||

                    (x.A.Name != null && (
                        x.A.Name.ToUpper() == searchU ||
                        x.A.Name.ToUpper().StartsWith(searchU) ||
                        EF.Functions.Like(x.A.Name.ToUpper(), likeU)
                    )) ||

                    // language-aware fields
                    (x.SubName != null && (
                        x.SubName.ToUpper() == searchU ||
                        x.SubName.ToUpper().StartsWith(searchU) ||
                        EF.Functions.Like(x.SubName.ToUpper(), likeU)
                    )) ||

                    (x.CatName != null && (
                        x.CatName.ToUpper() == searchU ||
                        x.CatName.ToUpper().StartsWith(searchU) ||
                        EF.Functions.Like(x.CatName.ToUpper(), likeU)
                    ))
                )
                .Select(x => new
                {
        
        ExactMask =
        (x.A.Barcode != null && x.A.Barcode.ToUpper() == searchU ? (int)AssetMatch.Barcode : 0) |
        (x.A.SerialNo != null && x.A.SerialNo.ToUpper() == searchU ? (int)AssetMatch.Serial : 0) |
        (x.A.Plate != null && x.A.Plate.ToUpper() == searchU ? (int)AssetMatch.Plate : 0) |
        (x.A.Engine != null && x.A.Engine.ToUpper() == searchU ? (int)AssetMatch.Engine : 0) |
        (x.A.Chasis != null && x.A.Chasis.ToUpper() == searchU ? (int)AssetMatch.Chasis : 0) |
        (x.A.Name != null && x.A.Name.ToUpper() == searchU ? (int)AssetMatch.Name : 0) |
        (x.A.Make != null && x.A.Make.ToUpper() == searchU ? (int)AssetMatch.Make : 0) |
        (x.A.Model != null && x.A.Model.ToUpper() == searchU ? (int)AssetMatch.Model : 0) |
        (x.SubName != null && x.SubName.ToUpper() == searchU ? (int)AssetMatch.Sub : 0) |
        (x.CatName != null && x.CatName.ToUpper() == searchU ? (int)AssetMatch.Cat : 0),

        PrefixMask =
        (x.A.SerialNo != null && x.A.SerialNo.ToUpper().StartsWith(searchU) ? (int)AssetMatch.Serial : 0) |
        (x.A.Plate != null && x.A.Plate.ToUpper().StartsWith(searchU) ? (int)AssetMatch.Plate : 0) |
        (x.A.Engine != null && x.A.Engine.ToUpper().StartsWith(searchU) ? (int)AssetMatch.Engine : 0) |
        (x.A.Chasis != null && x.A.Chasis.ToUpper().StartsWith(searchU) ? (int)AssetMatch.Chasis : 0) |
        (x.A.Name != null && x.A.Name.ToUpper().StartsWith(searchU) ? (int)AssetMatch.Name : 0) |
        (x.SubName != null && x.SubName.ToUpper().StartsWith(searchU) ? (int)AssetMatch.Sub : 0) |
        (x.CatName != null && x.CatName.ToUpper().StartsWith(searchU) ? (int)AssetMatch.Cat : 0) |
        (x.A.Make != null && x.A.Make.ToUpper().StartsWith(searchU) ? (int)AssetMatch.Make : 0) |
        (x.A.Model != null && x.A.Model.ToUpper().StartsWith(searchU) ? (int)AssetMatch.Model : 0),

        ContainsMask =
        (x.A.SerialNo != null && EF.Functions.Like(x.A.SerialNo.ToUpper(), likeU) ? (int)AssetMatch.Serial : 0) |
        (x.A.Plate != null && EF.Functions.Like(x.A.Plate.ToUpper(), likeU) ? (int)AssetMatch.Plate : 0) |
        (x.A.Engine != null && EF.Functions.Like(x.A.Engine.ToUpper(), likeU) ? (int)AssetMatch.Engine : 0) |
        (x.A.Chasis != null && EF.Functions.Like(x.A.Chasis.ToUpper(), likeU) ? (int)AssetMatch.Chasis : 0) |
        (x.A.Name != null && EF.Functions.Like(x.A.Name.ToUpper(), likeU) ? (int)AssetMatch.Name : 0) |
        (x.SubName != null && EF.Functions.Like(x.SubName.ToUpper(), likeU) ? (int)AssetMatch.Sub : 0) |
        (x.CatName != null && EF.Functions.Like(x.CatName.ToUpper(), likeU) ? (int)AssetMatch.Cat : 0) |
        (x.A.Make != null && EF.Functions.Like(x.A.Make.ToUpper(), likeU) ? (int)AssetMatch.Make : 0) |
        (x.A.Model != null && EF.Functions.Like(x.A.Model.ToUpper(), likeU) ? (int)AssetMatch.Model : 0),

        Score =
        (x.A.Barcode != null && x.A.Barcode.ToUpper() == searchU ? 300 : 0) +
        (x.A.SerialNo != null && x.A.SerialNo.ToUpper() == searchU ? 300 : 0) +
        (x.A.Plate != null && x.A.Plate.ToUpper() == searchU ? 300 : 0) +
        (x.A.Engine != null && x.A.Engine.ToUpper() == searchU ? 300 : 0) +
        (x.A.Chasis != null && x.A.Chasis.ToUpper() == searchU ? 300 : 0) +
        (x.A.Name != null && x.A.Name.ToUpper() == searchU ? 300 : 0) +
        (x.SubName != null && x.SubName.ToUpper() == searchU ? 250 : 0) +
        (x.CatName != null && x.CatName.ToUpper() == searchU ? 200 : 0) +
        (x.A.Make != null && x.A.Make.ToUpper() == searchU ? 120 : 0) +
        (x.A.Model != null && x.A.Model.ToUpper() == searchU ? 120 : 0) +

        (x.A.SerialNo != null && x.A.SerialNo.ToUpper().StartsWith(searchU) ? 30 : 0) +
        (x.A.Plate != null && x.A.Plate.ToUpper().StartsWith(searchU) ? 30 : 0) +
        (x.A.Engine != null && x.A.Engine.ToUpper().StartsWith(searchU) ? 30 : 0) +
        (x.A.Chasis != null && x.A.Chasis.ToUpper().StartsWith(searchU) ? 30 : 0) +
        (x.A.Name != null && x.A.Name.ToUpper().StartsWith(searchU) ? 30 : 0) +
        (x.SubName != null && x.SubName.ToUpper().StartsWith(searchU) ? 25 : 0) +
        (x.CatName != null && x.CatName.ToUpper().StartsWith(searchU) ? 20 : 0) +
        (x.A.Make != null && x.A.Make.ToUpper().StartsWith(searchU) ? 15 : 0) +
        (x.A.Model != null && x.A.Model.ToUpper().StartsWith(searchU) ? 15 : 0) +

        (x.A.SerialNo != null && EF.Functions.Like(x.A.SerialNo.ToUpper(), likeU) ? 3 : 0) +
        (x.A.Plate != null && EF.Functions.Like(x.A.Plate.ToUpper(), likeU) ? 3 : 0) +
        (x.A.Engine != null && EF.Functions.Like(x.A.Engine.ToUpper(), likeU) ? 3 : 0) +
        (x.A.Chasis != null && EF.Functions.Like(x.A.Chasis.ToUpper(), likeU) ? 3 : 0) +
        (x.A.Name != null && EF.Functions.Like(x.A.Name.ToUpper(), likeU) ? 3 : 0) +
        (x.SubName != null && EF.Functions.Like(x.SubName.ToUpper(), likeU) ? 3 : 0) +
        (x.CatName != null && EF.Functions.Like(x.CatName.ToUpper(), likeU) ? 3 : 0) +
        (x.A.Make != null && EF.Functions.Like(x.A.Make.ToUpper(), likeU) ? 2 : 0) +
        (x.A.Model != null && EF.Functions.Like(x.A.Model.ToUpper(), likeU) ? 2 : 0),
            DTO = new AssetSearchDTO
            {
                Id = x.A.Id,
                Category = x.CatName ?? string.Empty,
                SubCategory = x.SubName ?? string.Empty,
                Make = x.A.Make ?? string.Empty,
                Model = x.A.Model ?? string.Empty,
                Name = x.A.Name ?? string.Empty,
                Specifications = x.A.Specifications ?? string.Empty,
                StoreName = x.StoreName ?? string.Empty,
                Barcode = x.A.Barcode ?? string.Empty,
                SerialNo = x.A.SerialNo ?? string.Empty,
                IsVehicle = x.A.SubCategory.Category.Id == vehicleCategoryId,
                Plate = x.A.Plate ?? string.Empty,
                Engine = x.A.Engine ?? string.Empty,
                Chasis = x.A.Chasis ?? string.Empty,
                AssetPicture = _db.AssetPictures
                .Where(p => p.AssetId == x.A.Id)
                .OrderBy(p => p.Id)
                .Select(p => p.ImageUrl)
                .FirstOrDefault() ?? string.Empty
            }
        })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.DTO.Name)
            .Select(x => new AssetSearchDTO
            {
                Id = x.DTO.Id,
                Category = x.DTO.Category,
                SubCategory = x.DTO.SubCategory,
                Make = x.DTO.Make,
                Model = x.DTO.Model,
                Name = x.DTO.Name,
                Specifications = x.DTO.Specifications,
                StoreName = x.DTO.StoreName,
                Barcode = x.DTO.Barcode,
                SerialNo = x.DTO.SerialNo,
                IsVehicle = x.DTO.IsVehicle,
                Plate = x.DTO.Plate,
                Engine = x.DTO.Engine,
                Chasis = x.DTO.Chasis,
                AssetPicture = x.DTO.AssetPicture,
                MatchExactMask = x.ExactMask,
                MatchPrefixMask = x.PrefixMask,
                MatchContainsMask = x.ContainsMask
            })
            .Take(take)
            .ToListAsync();

            return results;
        }



        public async Task<List<vwCategoryAsset>> GetvwCategoryAssetsAsync()
        {
            var result = await _db.Assets
                .Include(m => m.Store)
                .Include(m => m.SubCategory).ThenInclude(m => m.Category)
                .Where(a => a.AssetStatusId != 4)
                .GroupBy(a => new
                {
                    a.SubCategory.Category.Id,
                    CategoryName = _isSomali ? a.SubCategory.Category.CategoryNameSo : a.SubCategory.Category.CategoryName
                })
                .Select(g => new vwCategoryAsset
                {
                    Id = g.Key.Id,
                    CategoryName = g.Key.CategoryName,
                    TotalAssets = g.Count(),
                    TotalCost = g.Sum(a => a.Cost)
                })
                .ToListAsync();

            return result;
        }
        public async Task<bool> IsStoreUser(int employeeId, int storeId)
        {
            var storeEmployee = await _db.StoreEmployees
                .Where(se => se.EmployeeId == employeeId && se.StoreId == storeId)
                .FirstOrDefaultAsync();
            return storeEmployee != null;
        }
        public void LogException(ILogger logger, Exception ex)
        {
            if (ex.InnerException != null)
            {
                logger.LogError($"Error: {ex.Message} | Inner Exception: {ex.InnerException.Message}");
            }
            else
            {
                logger.LogError($"Error: {ex.Message}");
            }
        }
        public async Task SendNotificationAsync(Notification notification)
        {
            await _db.Notifications.AddAsync(notification);
            await _db.SaveChangesAsync();
            await SendEmailAsync(notification.EmployeeTo, notification.Subject, notification.Message, notification.TargetRecordId, notification.NotificationSectionId);
        }
        public async Task<bool> CanModifyStoreAsync(int storeId, int employeeId)
        {
            if (await GetStoreOwnerIdAsync(storeId) == employeeId) return true;
            if (await IsStoreUser(employeeId, storeId)) return true;
            return false;
        }
        public async Task<List<Employee>> GetStoreEmployeesByStoreIdAsync(int storeId)
        {
            var employees = await _db.StoreEmployees
                .Where(se => se.StoreId == storeId)
                .Select(se => se.Employee)
                .ToListAsync();
            return employees;
        }

        //Private methods
        private async Task NotifyAsync(int[] arrEmpIds, Notification notification)
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
                    NotificationSectionId = notification.NotificationSectionId
                };
                newNotification.EmployeeTo = id;
                await _db.Notifications.AddAsync(newNotification);
                await _db.SaveChangesAsync();

                await SendEmailAsync(id, notification.Subject, notification.Message, notification.TargetRecordId, notification.NotificationSectionId);
            }
        }
        private async Task SendEmailAsync(int empId, string subject, string message, int recordId, int sectionId)
        {
            var emp = _db.Employees.Where(m => m.Id == empId).FirstOrDefault();
            string sEmail = "";
            string sFullName = "";

            if (emp != null)
            {
                sEmail = emp.Email;
                sFullName = emp.FullName;
            }

            var returnUrl = GenerateUrl(sectionId, recordId);
            string sEmailMessage = FormatMessage(subject, message, sFullName, returnUrl, _isSomali ? "Guji halkan" : "click here");

            try
            {
                await _emailSender.SendEmailAsync(
                sEmail,
                subject,
                sEmailMessage);

                _logger.LogInformation($"Email sent to {sEmail} with subject '{subject}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"error sending email: {ex.Message}");
            }
        }
        private string GenerateUrl(int notificationSectionId, int targetRecordId)
        {
            string? callbackUrl = "";

            var section = _db.NotificationSections.Where(m => m.Id == notificationSectionId).FirstOrDefault();

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


        //Depreciation Calculation
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
