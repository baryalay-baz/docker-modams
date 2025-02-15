﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MODAMS.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name ="Full Name")]
        public string FullName { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = String.Empty;

        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = String.Empty;

        [EmailAddress]
        [Required]
        [Display(Name ="Email Address")]
        public string Email { get; set; } = String.Empty;

        [Required]
        [Display(Name = "ID Card Number")]
        public string CardNumber { get; set; } = String.Empty;

        [Required]
        [Display(Name ="Supervisor")]
        public int SupervisorEmployeeId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name ="Role Name")]
        public string InitialRole { get; set; } = String.Empty;

        [AllowNull]
        public string ImageUrl { get; set; } = String.Empty;
        public string DisplayMode { get; set; } = String.Empty;

    }
}
