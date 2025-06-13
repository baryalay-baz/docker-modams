using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MODAMS.Models.ViewModels.Dto
{
    public class VerificationScheduleCreateDTO
    {
        [ValidateNever]
        public int StoreId { get; set; }

        [ValidateNever]
        public string StoreName { get; set; } = string.Empty;

        [ValidateNever]
        public string DepartmentName { get; set; } = string.Empty;

        public VerificationSchedule NewSchedule { get; set; } // For creating a new schedule

        [ValidateNever]
        public List<VerificationTeam> NewTeam { get; set; } = new(); // For adding a new team to the schedule

        [ValidateNever]
        public IEnumerable<SelectListItem> EmployeesList { get; set; } = new List<SelectListItem>();

        [ValidateNever]
        public List<Employee> Employees { get; set; } = new();

        [ValidateNever]
        public int NumberOfAssets { get; set; }

        [ValidateNever]
        public bool IsSomali { get; set; } = false;
    }
}
