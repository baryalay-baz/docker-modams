using Microsoft.AspNetCore.Mvc;
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
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name="Department Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name ="Upper-level Department")]
        public int UpperLevelDeptId { get; set; }

        [AllowNull]
        [Display(Name ="Department Owner")]
        public int EmployeeId { get; set; }

       
        public int DisplayOrder { get; set; }
    }
}
