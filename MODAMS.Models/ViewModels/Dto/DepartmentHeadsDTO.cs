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
    public class DepartmentHeadsDTO
    {
        [Display(Name = "Select Employee to Assign as Owner")]
        public int EmployeeId { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Owner {  get; set; }
        public List<vwEmployees> DepartmentUsers { get; set; }

        public List<DepartmentHead> DepartmentHeads { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Employees { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Months { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Years { get; set; }


    }
}
