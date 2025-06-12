using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MODAMS.ApplicationServices.IServices;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using Newtonsoft.Json;
using System.Globalization;


namespace MODAMS.ApplicationServices
{
    public class DepartmentsService : IDepartmentsService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly ILogger<DepartmentsService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly int _employeeId;
        private readonly bool _isSomali;
        public DepartmentsService(ApplicationDbContext db, IAMSFunc func, ILogger<DepartmentsService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _func = func;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            _employeeId = _func.GetEmployeeId();
            _isSomali = CultureInfo.CurrentCulture.Name == "so";
        }
        public async Task<Result<DepartmentsDTO>> GetIndexAsync()
        {
            try
            {
                List<vwDepartments> depts = await _db.vwDepartments
                    .OrderByDescending(m => m.EmployeeId).ToListAsync();

                if (IsInRole("StoreOwner"))
                {
                    depts = depts.Where(m => m.EmployeeId == _employeeId).ToList();
                }

                var storeUsers = await _db.StoreEmployees
                    .Select(m => new StoreUsersDTO
                    {
                        Id = m.EmployeeId,
                        FullName = m.Employee.FullName,
                        JobTitle = m.Employee.JobTitle,
                        SupervisorEmployeeId = m.Employee.SupervisorEmployeeId,
                        ImageUrl = m.Employee.ImageUrl ?? "/assets/images/faces/profile_placeholder.png",
                        Email = m.Employee.Email,
                        Phone = m.Employee.Phone,
                        StoreId = m.StoreId,
                        DepartmentId = m.Store.DepartmentId
                    })
                    .ToListAsync();


                var dto = new DepartmentsDTO()
                {
                    departments = depts,
                    storeUsers = storeUsers
                };
                return Result<DepartmentsDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<DepartmentsDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<DepartmentDTO>> GetCreateDepartmentAsync()
        {
            try
            {
                var departmentDto = new DepartmentDTO()
                {
                    department = new Department()
                };

                departmentDto = await PopulateDepartmentDTO(departmentDto);

                return Result<DepartmentDTO>.Success(departmentDto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<DepartmentDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<DepartmentDTO>> CreateDepartmentAsync(DepartmentDTO dto)
        {
            try
            {
                var department = await _db.Departments
                .Where(m => m.Name == dto.department.Name).FirstOrDefaultAsync();

                if (department != null)
                {
                    dto = await PopulateDepartmentDTO(dto);
                    return Result<DepartmentDTO>.Failure(_isSomali ? "Waaxda horay ayey u jirtay!" : "Department already exists!", dto);
                }
                await _db.Departments.AddAsync(dto.department);
                await _db.SaveChangesAsync();
                await CreateStoreAsync(dto.department);
                dto = await PopulateDepartmentDTO(dto);
                return Result<DepartmentDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                dto = await PopulateDepartmentDTO(dto);
                return Result<DepartmentDTO>.Failure(ex.Message, dto);
            }
        }
        public async Task<Result<DepartmentDTO>> GetEditDepartmentAsync(int departmentId)
        {
            try
            {
                var department = await _db.Departments.FirstOrDefaultAsync(m => m.Id == departmentId);
                string departmentOwner = "";

                if (department == null)
                    return Result<DepartmentDTO>.Failure(_isSomali ? "Waaxda lama helin!" : "Department not found!");

                departmentOwner = await _func.GetEmployeeNameByIdAsync(department.EmployeeId == 0 || department.EmployeeId == null ? 0 : (int)department.EmployeeId);
                var dto = new DepartmentDTO()
                {
                    department = department,
                    DepartmentOwner = departmentOwner
                };

                dto = await PopulateDepartmentDTO(dto);

                return Result<DepartmentDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<DepartmentDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<DepartmentDTO>> EditDepartmentAsync(DepartmentDTO dto)
        {
            try
            {
                var department = await _db.Departments.FirstOrDefaultAsync(m => m.Id == dto.department.Id);
                if (department == null)
                {
                    dto = await PopulateDepartmentDTO(dto);
                    return Result<DepartmentDTO>.Failure(_isSomali ? "Waaxda lama helin!" : "Department not found!", dto);
                }
                department.Name = dto.department.Name;
                department.NameSo = dto.department.NameSo;
                department.UpperLevelDeptId = dto.department.UpperLevelDeptId;

                await _db.SaveChangesAsync();
                await UpdateStoreAsync(department);

                dto = await PopulateDepartmentDTO(dto);
                return Result<DepartmentDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                dto = await PopulateDepartmentDTO(dto);
                return Result<DepartmentDTO>.Failure(ex.Message, dto);
            }
        }
        public async Task<Result<List<OrganizationChartDTO>>> GetOrganizationChartAsync()
        {
            try
            {
                var departments = await _db.vwDepartments.OrderBy(m => m.Id).ToListAsync();

                List<OrganizationChartDTO> dto = new List<OrganizationChartDTO>();
                int nCounter = 0;
                foreach (var item in departments)
                {
                    nCounter++;
                    string imageUrl = (item.ImageUrl == string.Empty) ? "/assets/images/faces/profile_placeholder.png" : item.ImageUrl;
                    int? parentId = (item.UpperLevelDeptId == 0) ? null : item.UpperLevelDeptId;

                    var orgChart = new OrganizationChartDTO()
                    {
                        ID = item.Id,
                        Name = _isSomali ? item.NameSo : item.Name,
                        ParentID = parentId,
                        Title = item.OwnerName,
                        Avatar = imageUrl,
                        Expanded = false
                    };
                    dto.Add(orgChart);

                }
                return Result<List<OrganizationChartDTO>>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<List<OrganizationChartDTO>>.Failure(ex.Message);
            }
        }
        public async Task<Result<DepartmentHeadsDTO>> GetDepartmentHeadsAsync(int departmentId)
        {

            if (IsInRole("StoreOwner"))
            {
                var storeId = await _func.GetStoreIdByDepartmentIdAsync(departmentId);
                if (!await _func.CanModifyStoreAsync(storeId, _employeeId))
                {
                    return Result<DepartmentHeadsDTO>.Failure(_isSomali ? "Uma haysid oggolaansho inaad waaxdan wax ka beddesho" : "You are not authorized to modify this department");
                }
            }
            try
            {
                var department = await _db.Departments
                    .Include(d => d.Stores)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(d => d.Id == departmentId);

                if (department == null)
                    return Result<DepartmentHeadsDTO>.Failure(_isSomali ? "Waaxda lama helin" : "Department not found");

                int storeId = department.Stores.FirstOrDefault()?.Id ?? 0;

                var departmentHeads = await _db.DepartmentHeads
                    .Where(h => h.DepartmentId == departmentId)
                    .Include(h => h.Employee)
                    .OrderByDescending(h => h.StartDate)
                    .AsNoTracking()
                    .ToListAsync();

                var availableStoreOwners = await _db.vwAvailableEmployees
                    .Where(e => e.RoleName == "StoreOwner")
                    .OrderBy(e => e.FullName)
                    .Select(e => new SelectListItem
                    {
                        Text = $"{e.FullName} - {e.JobTitle}",
                        Value = e.Id.ToString()
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var availableStoreUsers = await _db.vwEmployees
                    .Where(e =>
                        e.RoleName == "User"
                        && !_db.StoreEmployees.Any(se => se.EmployeeId == e.Id))
                    .OrderBy(e => e.FullName)
                    .Select(e => new SelectListItem
                    {
                        Text = $"{e.FullName} - {e.JobTitle}",
                        Value = e.Id.ToString()
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var storeUsers = await _db.StoreEmployees
                    .Where(se => se.StoreId == storeId)
                    .Select(se => new vwEmployees
                    {
                        Id = se.EmployeeId,
                        FullName = se.Employee.FullName,
                        JobTitle = se.Employee.JobTitle,
                        SupervisorEmployeeId = se.Employee.SupervisorEmployeeId,
                        ImageUrl = se.Employee.ImageUrl ?? "/assets/images/faces/profile_placeholder.png",
                        Email = se.Employee.Email,
                        Phone = se.Employee.Phone
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var currentHead = departmentHeads.FirstOrDefault();
                string owner = currentHead?.Employee.FullName ?? "Vacant";

                var dto = new DepartmentHeadsDTO
                {
                    DepartmentHeads = departmentHeads,
                    AvailableStoreOwners = availableStoreOwners,
                    AvailableUsers = availableStoreUsers,
                    DepartmentId = departmentId,
                    StoreId = storeId,
                    DepartmentName = department.Name,
                    Owner = owner,
                    DepartmentUsers = storeUsers
                };

                return Result<DepartmentHeadsDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<DepartmentHeadsDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<bool>> AssignOwnerAsync(DepartmentHeadsDTO dto)
        {
            try
            {
                int nDepartmentId = dto.DepartmentId;
                int nEmployeeId = dto.EmployeeId;

                if (nEmployeeId == 0)
                {
                    return Result<bool>.Failure(_isSomali ? "Shaqaale lama helin!" : "Employee not found!");
                }

                var departmentHead = await _db.DepartmentHeads
                    .FirstOrDefaultAsync(m => m.DepartmentId == nDepartmentId && m.IsActive);

                if (departmentHead != null)
                {
                    departmentHead.IsActive = false;
                    departmentHead.EndDate = DateTime.Now;

                    var departmentUsers = await _db.Employees.Where(m => m.SupervisorEmployeeId == departmentHead.EmployeeId).ToListAsync();
                    if (departmentUsers != null)
                    {
                        foreach (var user in departmentUsers)
                        {
                            user.SupervisorEmployeeId = nEmployeeId;
                        }
                    }
                }

                if (nEmployeeId != 0)
                {
                    var newDepartmentHead = new DepartmentHead
                    {
                        DepartmentId = nDepartmentId,
                        EmployeeId = nEmployeeId,
                        StartDate = DateTime.Now,
                        IsActive = true
                    };
                    await _db.DepartmentHeads.AddAsync(newDepartmentHead);
                }

                var department = await _db.Departments.FirstOrDefaultAsync(m => m.Id == nDepartmentId);
                if (department != null)
                {
                    department.EmployeeId = nEmployeeId != 0 ? nEmployeeId : null;
                }

                await _db.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<bool>.Failure(ex.Message, false);
            }
        }
        public async Task<Result<bool>> NewUserAsync(DepartmentHeadsDTO dto)
        {
            var employeeId = dto.EmployeeId;
            var storeId = dto.StoreId;

            try
            {
                if (employeeId == 0 || storeId == 0)
                {
                    return Result<bool>.Failure(_isSomali ? "Shaqaale lama helin!" : "Employee not found!");
                }
                var storeEmployee = await _db.StoreEmployees
                    .FirstOrDefaultAsync(m => m.StoreId == storeId && m.EmployeeId == employeeId);
                if (storeEmployee != null)
                {
                    return Result<bool>.Failure(_isSomali ? "Shaqaalahan horey ayuu u jiray!" : "This employee already exists in this store!");
                }
                storeEmployee = new StoreEmployee()
                {
                    StoreId = storeId,
                    EmployeeId = employeeId
                };
                await _db.StoreEmployees.AddAsync(storeEmployee);
                await _db.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<bool>.Failure(ex.Message, false);
            }

        }
        public async Task<Result<bool>> VacateDepartmentAsync(DepartmentHeadsDTO dto)
        {
            try
            {
                var department = await _db.Departments.FirstOrDefaultAsync(m => m.Id == dto.DepartmentId);
                if (department != null)
                {
                    department.EmployeeId = null;

                    var departmentHead = _db.DepartmentHeads.FirstOrDefault(m => m.DepartmentId == dto.DepartmentId && m.IsActive == true);
                    if (departmentHead != null)
                    {
                        departmentHead.EndDate = DateTime.Now;
                        departmentHead.IsActive = false;
                    }
                    await _db.SaveChangesAsync();

                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<bool>.Failure(ex.Message);
            }
        }
        public async Task<string> GetDepartmentsAsync()
        {
            string sResult = "No Records Found";
            var departments = await _db.vwDepartments.ToListAsync();
            if (departments != null)
            {
                sResult = JsonConvert.SerializeObject(departments);
            }
            return sResult;
        }
        public async Task<List<OrganizationChartDTO>> GetOrganizationChartJsonAsync()
        {
            var departments = await _db.vwDepartments.OrderBy(m => m.Id).ToListAsync();

            List<OrganizationChartDTO> dto = new List<OrganizationChartDTO>();
            int nCounter = 0;
            foreach (var item in departments)
            {
                nCounter++;
                string imageUrl = string.IsNullOrEmpty(item.ImageUrl) ? "/assets/images/faces/profile_placeholder.png" : item.ImageUrl;
                int? parentId = (item.UpperLevelDeptId == 0) ? null : item.UpperLevelDeptId;

                dto.Add(new OrganizationChartDTO()
                {
                    ID = item.Id,
                    Name = item.Name,
                    ParentID = parentId,
                    Title = item.OwnerName,
                    Avatar = imageUrl,
                    Expanded = false
                });
            }

            return dto;
        }
        public async Task<DepartmentDTO> PopulateDepartmentDTO(DepartmentDTO dto)
        {

            var employeeList = await _db.Employees.Select(m => new SelectListItem
            {
                Text = m.FullName,
                Value = m.Id.ToString()
            }).ToListAsync();

            var departmentList = await _db.vwDepartments.Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            }).ToListAsync();

            dto.Employees = employeeList;
            dto.Departments = departmentList;

            return dto;
        }
        public async Task<Result<bool>> RemoveDepartmentUserAsync(int employeeId, int storeId)
        {
            try
            {
                var storeEmployee = await _db.StoreEmployees
                    .FirstOrDefaultAsync(m => m.StoreId == storeId && m.EmployeeId == employeeId);

                if (storeEmployee != null)
                {
                    _db.StoreEmployees.Remove(storeEmployee);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    return Result<bool>.Failure(_isSomali ? "Shaqaalahan lama helin!" : "Employee not found in this store!");
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<bool>.Failure(ex.Message, false);
            }
        }


        //private functions
        private bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        private async Task CreateStoreAsync(Department department)
        {
            var store = new Store()
            {
                Name = department.Name,
                NameSo = department.NameSo,
                Description = "Store for " + department.Name,
                DepartmentId = department.Id
            };
            await _db.Stores.AddAsync(store);
            await _db.SaveChangesAsync();
        }
        private async Task UpdateStoreAsync(Department department)
        {
            var storeInDb = await _db.Stores.FirstOrDefaultAsync(m => m.DepartmentId == department.Id);

            if (storeInDb != null)
            {
                storeInDb.Name = department.Name;
                storeInDb.NameSo = department.NameSo;
                storeInDb.Description = "Store for " + department.Name;
            }
            await _db.SaveChangesAsync();
        }
    }
}
