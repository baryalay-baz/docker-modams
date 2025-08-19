using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MODAMS.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class ProfileDataDTO
    {
        public int Id { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "FullName", ResourceType = typeof(ProfileDataLabels))]
        public string FullName { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "JobTitle", ResourceType = typeof(ProfileDataLabels))]
        public string JobTitle { get; set; } = string.Empty;

        [ValidateNever]
        [Display(Name = "Department", ResourceType = typeof(ProfileDataLabels))]
        public string Department { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Email", ResourceType = typeof(ProfileDataLabels))]
        public string Email { get; set; } = string.Empty;

        [ValidateNever]
        [Display(Name = "Phone", ResourceType = typeof(ProfileDataLabels))]
        public string Phone { get; set; } = string.Empty;

        [ValidateNever]
        [Display(Name = "ImageUrl", ResourceType = typeof(ProfileDataLabels))]
        public string ImageUrl { get; set; } = string.Empty;

        [ValidateNever]
        [Display(Name = "RoleName", ResourceType = typeof(ProfileDataLabels))]
        public string RoleName { get; set; } = string.Empty;
        [ValidateNever]
        public int SupervisorEmployeeId { get; set; }

        [ValidateNever]
        [Display(Name = "SupervisorName", ResourceType = typeof(ProfileDataLabels))]
        public string SupervisorName { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "CardNumber", ResourceType = typeof(ProfileDataLabels))]
        public string CardNumber { get; set; } = string.Empty;
    }
}
