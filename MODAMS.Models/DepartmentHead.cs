using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class DepartmentHead
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name ="Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ValidateNever]
        public Employee Employee { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [ValidateNever]
        public Department Department { get; set; }

    }
}
