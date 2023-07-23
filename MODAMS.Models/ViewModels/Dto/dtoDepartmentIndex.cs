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
    public class dtoDepartmentIndex
    {
        public List<vwDepartments> DepartmentList { get; set; }

        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Employees { get; set; }

    }
}
