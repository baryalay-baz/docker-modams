using MODAMS.Models.ViewModels.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices.IServices
{
    public interface IDepartmentsService
    {
        Task<Result<DepartmentsDTO>> GetIndexAsync();
        Task<Result<DepartmentDTO>> GetCreateDepartmentAsync();
        Task<Result<DepartmentDTO>> CreateDepartmentAsync(DepartmentDTO dto);
        Task<Result<DepartmentDTO>> GetEditDepartmentAsync(int departmentId);
        Task<Result<DepartmentDTO>> EditDepartmentAsync(DepartmentDTO dto);
        Task<Result<List<OrganizationChartDTO>>> GetOrganizationChartAsync();
        Task<Result<DepartmentHeadsDTO>> GetDepartmentHeadsAsync(int departmentId);
        Task<Result<bool>> AssignOwnerAsync(DepartmentHeadsDTO dto);
        Task<Result<bool>> VacateDepartmentAsync(DepartmentHeadsDTO dto);
        Task<Result<bool>> NewUserAsync(DepartmentHeadsDTO dto);
        Task<string> GetDepartmentsAsync();
        Task<List<OrganizationChartDTO>> GetOrganizationChartJsonAsync();
        Task<DepartmentDTO> PopulateDepartmentDTO(DepartmentDTO dto);
        Task<Result<bool>> RemoveDepartmentUserAsync(int storeId, int employeeId);
    }
}
