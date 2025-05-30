using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MODAMS.DataAccess.Data;
using MODAMS.Models.ViewModels;
using MODAMS.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MODAMS.Models.ViewModels.Dto;
using Microsoft.AspNetCore.Mvc.Rendering;
using MODAMS.Models;
using System.Text.Encodings.Web;
using Newtonsoft.Json;
using MODAMS.ApplicationServices.IServices;
using System.Globalization;

namespace MODAMS.ApplicationServices
{
    public class EmployeesService : IEmployeesService
    {
        private readonly ILogger<EmployeesService> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int _employeeId;
        private readonly bool _isSomali;
        public EmployeesService(ILogger<EmployeesService> logger, ApplicationDbContext db, IAMSFunc func,
            UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _db = db;
            _func = func;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;

            _employeeId = _func.GetEmployeeId();
            _isSomali = CultureInfo.CurrentCulture.Name == "so";
        }

        private bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;

        public async Task<Result<List<vwEmployees>>> GetIndexAsync()
        {
            try
            {
                var employees = await _db.vwEmployees.ToListAsync();

                if (IsInRole(SD.Role_StoreOwner))
                {
                    employees = employees.Where(m => m.SupervisorEmployeeId == _employeeId).ToList();
                }

                return Result<List<vwEmployees>>.Success(employees);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<List<vwEmployees>>.Failure(ex.Message);
            }
        }
        public async Task<Result<EmployeeDTO>> GetCreateEmployeeAsync()
        {
            try
            {
                var employeeDto = new EmployeeDTO
                {
                    Employee = new Employee() // Initialize the Employee object
                };

                var dto = await PopulateEmployeeDTOAsync(employeeDto);

                return Result<EmployeeDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<EmployeeDTO>.Failure($"An error occurred: {ex.Message}");
            }
        }
        public async Task<Result<EmployeeDTO>> CreateEmployeeAsync(EmployeeDTO dto)
        {
            try
            {
                var employeeInDb = await _db.Employees.FirstOrDefaultAsync(m => m.Email == dto.Employee.Email);

                if (employeeInDb != null)
                    return Result<EmployeeDTO>.Failure(_isSomali? "Shaqaale leh email-kan hore ayuu u jiray" : "Employee with this email already exists");

                dto.Employee.ImageUrl = "/assets/images/faces/profile_placeholder.png";
                dto.Employee.IsActive = true;

                await _db.Employees.AddAsync(dto.Employee);
                await _db.SaveChangesAsync();
                dto = await PopulateEmployeeDTOAsync(dto);
                return Result<EmployeeDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                dto = await PopulateEmployeeDTOAsync(dto);
                return Result<EmployeeDTO>.Failure(ex.Message, dto);
            }
        }
        public async Task<Result<EmployeeDTO>> GetEditEmployeeAsync(int employeeId)
        {
            try
            {
                var employeeDto = new EmployeeDTO();
                var employeeInDb = await _db.Employees.Where(m => m.Id == employeeId).FirstOrDefaultAsync();

                if (employeeInDb == null)
                    return Result<EmployeeDTO>.Failure(_isSomali? "Shaqaale lama helin!" : "Employee not found!");

                var currentRole = await _func.GetRoleNameAsync(employeeId);

                employeeDto.Employee = employeeInDb;
                employeeDto.roleId = currentRole;

                employeeDto = await PopulateEmployeeDTOAsync(employeeDto);

                return Result<EmployeeDTO>.Success(employeeDto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<EmployeeDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<EmployeeDTO>> EditEmployeeAsync(EmployeeDTO dto)
        {
            try
            {
                var employee = await _db.Employees.FirstOrDefaultAsync(m => m.Email == dto.Employee.Email);

                if (employee == null)
                {
                    dto = await PopulateEmployeeDTOAsync(dto);
                    return Result<EmployeeDTO>.Failure(_isSomali? "Shaqaale lama helin!" : "Employee not found!", dto);
                }

                employee.FullName = dto.Employee.FullName;
                employee.JobTitle = dto.Employee.JobTitle;
                employee.CardNumber = dto.Employee.CardNumber;
                employee.SupervisorEmployeeId = dto.Employee.SupervisorEmployeeId;
                employee.Phone = dto.Employee.Phone;

                var user = await _userManager.FindByEmailAsync(employee.Email);
                if (user != null)
                {
                    var role = await _userManager.GetRolesAsync(user);
                    string roleName = "User";

                    if (role != null)
                    {
                        roleName = role[0];
                    }

                    var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, dto.roleId);
                    }
                }
                await _db.SaveChangesAsync();
                dto = await PopulateEmployeeDTOAsync(dto);
                return Result<EmployeeDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                dto = await PopulateEmployeeDTOAsync(dto);
                return Result<EmployeeDTO>.Failure(ex.Message, dto);
            }
        }
        public async Task<Result<bool>> LockAccountAsync(int employeeId)
        {
            try
            {
                var employee = _db.Employees.Where(m => m.Id == employeeId).FirstOrDefault();

                if (employee == null)
                    return Result<bool>.Failure(_isSomali? "Shaqaale lama helin!" : "Employee not found!");

                employee.IsActive = false;
                await _db.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<bool>.Failure(ex.Message);
            }
        }
        public async Task<Result<bool>> UnLockAccountAsync(int employeeId)
        {
            try
            {
                var employee = _db.Employees.Where(m => m.Id == employeeId).FirstOrDefault();

                if (employee == null)
                    return Result<bool>.Failure(_isSomali ? "Shaqaale lama helin!" : "Employee not found!");

                employee.IsActive = true;
                await _db.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<bool>.Failure(ex.Message);
            }
        }
        public async Task<Result<string>> GetFacesAsync()
        {
            try
            {
                var sResult = "No Records Found";

                var faces = await _db.Employees.Select(m => new { m.Id, m.ImageUrl }).ToListAsync();
                if (faces.Count > 0)
                {
                    sResult = JsonConvert.SerializeObject(faces);
                }
                return Result<string>.Success(sResult);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<string>.Failure(ex.Message);
            }
        }
        public async Task SendRegistrationNotification(string emailAddress, string? baseUrl, string scheme)
        {
            // Construct the callback URL manually
            var callbackUrl = $"{scheme}://{baseUrl}/Identity/Account/Register?returnUrl={Uri.EscapeDataString(emailAddress)}";

            string shortMessage = _isSomali
            ? "Akoon cusub ayaa lagu abuuray nidaamka Maaraynta Hantida ee MOD, fadlan guji batoonka hoose si aad u raacdo tilmaamaha!"
            : "A new account has been created for you at MOD Asset Management System, please click the button below to follow the instructions!";

            string message;

            if (!string.IsNullOrEmpty(callbackUrl))
            {
                message = _func.FormatMessage(_isSomali? "Diiwaangelinta Akoonka" : "Account Registration", shortMessage, emailAddress,
                                              HtmlEncoder.Default.Encode(callbackUrl), "Register here");
            }
            else
            {
                message = _func.FormatMessage(_isSomali? "Dib u Deji Erayga Sirta" : "Reset Password", shortMessage, emailAddress,
                                              HtmlEncoder.Default.Encode("./"), "Register here");
            }

            await _emailSender.SendEmailAsync(emailAddress, _isSomali ? "Diiwaangelinta Akoonka" : "Account Registration", message);
        }
        public async Task<EmployeeDTO> PopulateEmployeeDTOAsync(EmployeeDTO dto)
        {
            var roleList = await _db.Roles
                    .Select(m => new SelectListItem
                    {
                        Text = m.Name,
                        Value = m.Name
                    })
                    .ToListAsync();

            // Fetch employee list
            var employeeList = await _db.Employees
                .Select(m => new SelectListItem
                {
                    Text = m.FullName,
                    Value = m.Id.ToString()
                })
                .ToListAsync();


            if (IsInRole("StoreOwner"))
            {
                roleList = roleList.Where(m => m.Text == "User").ToList();
                employeeList = employeeList.Where(m => m.Value == _employeeId.ToString()).ToList();
            }
            else if(IsInRole("SeniorManagement"))
            {
                roleList = roleList.Where(m => m.Text != "Administrator").ToList();
            }



            // Assign the fetched data to the DTO
            dto.Employees = employeeList;
            dto.RoleList = roleList;

            return dto;
        }
    }
}
