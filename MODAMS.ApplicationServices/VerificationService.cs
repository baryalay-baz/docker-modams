using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using Newtonsoft.Json;

namespace MODAMS.ApplicationServices
{
    public class VerificationService : IVerificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly ILogger<VerificationService> _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly int _employeeId;
        private readonly int _countryOfficeId;

        public VerificationService(ApplicationDbContext db, IAMSFunc func, IHttpContextAccessor httpContextAccessor, ILogger<VerificationService> logger)
        {
            _db = db;
            _func = func;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            _employeeId = _func.GetEmployeeId();
        }

        public async Task<Result<VerificationsDTO>> GetIndexAsync()
        {
            try
            {
                var dto = new VerificationsDTO();
                var schedules = await _db.VerificationSchedules
                    .Include(m => m.Store).ThenInclude(m => m.Department)
                    .Include(m => m.VerificationTeams).ThenInclude(m => m.Employee)
                    .ToListAsync();

                if (IsInRole(SD.Role_User))
                {
                    var _supervisorEmployeeId = await _func.GetSupervisorIdAsync(_employeeId);
                    schedules = schedules
                        .Where(m => m.Store.Department.EmployeeId == _supervisorEmployeeId)
                        .ToList();
                }
                else if (IsInRole(SD.Role_StoreOwner))
                {
                    schedules = schedules
                        .Where(m => m.Store.Department.EmployeeId == _employeeId)
                        .ToList();
                }
                dto.Schedules = schedules;
                return Result<VerificationsDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<VerificationsDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<VerificationScheduleCreateDTO>> GetCreateScheduleAsync()
        {
            try
            {
                var dto = new VerificationScheduleCreateDTO();

                var store = await _db.Stores
                .Where(m => m.Department.EmployeeId == _employeeId).FirstOrDefaultAsync();
                
                if (store != null)
                {
                    dto.StoreId = store.Id;
                    dto.Store = store;
                    var assets = await _db.Assets.Where(m => m.StoreId == store.Id).ToListAsync();

                    dto.NumberOfAssets = assets.Count;
                }
                else {
                    return Result<VerificationScheduleCreateDTO>.Failure("Store not available");
                }

                var employees = await _db.Employees.ToListAsync();

                if (IsInRole(SD.Role_User))
                {
                    int _supervisorEmployeeId = await _func.GetSupervisorIdAsync(_employeeId);
                    employees = employees.Where(m => m.SupervisorEmployeeId == _supervisorEmployeeId).ToList();
                }
                else if (IsInRole(SD.Role_StoreOwner))
                {
                    employees = employees.Where(m => (m.Id == _employeeId) || (m.SupervisorEmployeeId == _employeeId)).ToList();
                }

                dto.Employees = employees;
                dto.EmployeesList = employees
                    .Select(m => new SelectListItem
                    {
                        Text = $"{m.FullName} ({m.JobTitle})",
                        Value = m.Id.ToString()
                    });

                return Result<VerificationScheduleCreateDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<VerificationScheduleCreateDTO>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> CompleteVerificationSchedule(int scheduleId)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<VerificationScheduleCreateDTO>> CreateScheduleAsync(VerificationScheduleCreateDTO dto, string teamMembersData)
        {
            throw new NotImplementedException();
        }

        public async Task<Result> DeleteScheduleAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Result> DeleteVerificationRecordAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<VerificationScheduleEditDTO>> EditScheduleAsync(VerificationScheduleEditDTO dto, string teamMembersData)
        {
            throw new NotImplementedException();
        }



        public async Task<Result<VerificationScheduleEditDTO>> GetEditScheduleAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<VerificationSchedulePreviewDTO>> GetPreviewScheduleAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<bool>> VerifyAssetAsync(VerificationSchedulePreviewDTO dto, IFormFile file)
        {
            throw new NotImplementedException();
        }

        private bool IsInRole(string role) => _httpContextAccessor.HttpContext.User.IsInRole(role);

    }
}
