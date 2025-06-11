using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MODAMS.Localization;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MODAMS.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "FullName", ResourceType = typeof(EmployeeLabels))]
        public string FullName { get; set; } = String.Empty;
        [Required(
        ErrorMessageResourceType = typeof(ValidationMessages),
        ErrorMessageResourceName = "Required")]
        [Display(Name = "JobTitle", ResourceType = typeof(EmployeeLabels))]
        public string JobTitle { get; set; } = String.Empty;

        [Display(Name = "Phone", ResourceType = typeof(EmployeeLabels))]
        public string Phone { get; set; } = String.Empty;

        [EmailAddress]
        [Required(
        ErrorMessageResourceType = typeof(ValidationMessages),
        ErrorMessageResourceName = "Required")]
        [Display(Name = "Email", ResourceType = typeof(EmployeeLabels))]
        public string Email { get; set; } = String.Empty;
        [Required(
        ErrorMessageResourceType = typeof(ValidationMessages),
        ErrorMessageResourceName = "Required")]
        [Display(Name = "CardNumber", ResourceType = typeof(EmployeeLabels))]
        public string CardNumber { get; set; } = String.Empty;
        [Required(
        ErrorMessageResourceType = typeof(ValidationMessages),
        ErrorMessageResourceName = "Required")]
        [Display(Name = "Supervisor", ResourceType = typeof(EmployeeLabels))]
        public int SupervisorEmployeeId { get; set; }
                
        [Display(Name = "IsActive", ResourceType = typeof(EmployeeLabels))]
        public bool IsActive { get; set; } = true;
                
        [Display(Name = "InitialRole", ResourceType = typeof(EmployeeLabels))]
        public string InitialRole { get; set; } = String.Empty;

        [AllowNull]
        [Display(Name = "ImageUrl", ResourceType = typeof(EmployeeLabels))]
        public string ImageUrl { get; set; } = String.Empty;
        public string DisplayMode { get; set; } = String.Empty;

        [ValidateNever]
        public ICollection<StoreEmployee> StoreEmployees { get; set; }


    }
}
