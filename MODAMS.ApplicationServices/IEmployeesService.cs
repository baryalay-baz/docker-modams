using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices
{
    public interface IEmployeesService
    {
        Task<Result<List<vwEmployees>>> GetIndexAsync();
        Task<Result<EmployeeDTO>> GetCreateEmployeeAsync();
        Task<Result<EmployeeDTO>> CreateEmployeeAsync(EmployeeDTO dto);
        Task<Result<EmployeeDTO>> GetEditEmployeeAsync(int employeeId);
        Task<Result<EmployeeDTO>> EditEmployeeAsync(EmployeeDTO dto);
        Task<Result<bool>> LockAccountAsync(int employeeId);
        Task<Result<bool>> UnLockAccountAsync(int employeeId);
        Task<Result<string>> GetFacesAsync();
        Task SendRegistrationNotification(string emailAddress, string? baseUrl, string scheme);
        Task<EmployeeDTO> PopulateEmployeeDTOAsync(EmployeeDTO dto);
    }
}
