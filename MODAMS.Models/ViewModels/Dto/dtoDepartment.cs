using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoDepartment
    {
        public Department department { get; set; }


        [Display(Name ="Department Head")]
        [ValidateNever]
        public string DepartmentOwner { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Employees { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Departments { get; set; }
    }
}
