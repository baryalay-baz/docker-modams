using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class dtoEmployee
    {
        public Employee Employee { get; set; }


        [Display(Name ="Employee Role")]
        [ValidateNever]
        public string roleId { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Employees { get; set;}

        [ValidateNever]
        public IEnumerable<SelectListItem> RoleList { get; set;}
    }
}
