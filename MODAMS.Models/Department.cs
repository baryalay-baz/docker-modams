using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MODAMS.Localization;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MODAMS.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "DepartmentName", ResourceType = typeof(DepartmentLabels))]
        public string Name { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "DepartmentNameSo", ResourceType = typeof(DepartmentLabels))]
        public string NameSo { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "UpperLevelDepartment", ResourceType = typeof(DepartmentLabels))]
        public int UpperLevelDeptId { get; set; }

        [AllowNull]
        [Display(Name = "DepartmentOwner", ResourceType = typeof(DepartmentLabels))]
        public int? EmployeeId { get; set; }

        [ValidateNever]
        public ICollection<Store> Stores { get; set; }

    }
}
