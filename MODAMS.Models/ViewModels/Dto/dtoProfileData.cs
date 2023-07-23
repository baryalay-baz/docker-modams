using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoProfileData
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;

        [ValidateNever]
        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [ValidateNever]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [ValidateNever]
        [Display(Name = "Image Url")]
        public string ImageUrl { get; set; } = string.Empty;

        [ValidateNever]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;
        [ValidateNever]
        public int SupervisorEmployeeId { get; set; }

        [ValidateNever]
        [Display(Name = "Supervisor Name")]
        public string SupervisorName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "ID Card Number")]
        public string CardNumber { get; set; } = string.Empty;
    }
}
