using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
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
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        
        [Display(Name = "End Date")]
        [AllowNull]
        public DateTime? EndDate { get; set; }

        [Required]
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }


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
