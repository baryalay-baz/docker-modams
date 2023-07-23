using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoDepartment
    {
        public Department department { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Employees { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Departments { get; set; }
    }
}
