using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class Store
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name="Store Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name ="Description")]
        public string Description { get; set; } = String.Empty;

        [Required]
        [Display(Name ="Department")]
        public int DepartmentId { get; set; }

        [ValidateNever]
        public Department Department { get; set; }


        //Sub tables
        [ValidateNever]
        public ICollection<VerificationSchedule> VerificationSchedules { get; set; }
        [ValidateNever]
        public ICollection<Asset> Assets { get; set; }

    }
}
