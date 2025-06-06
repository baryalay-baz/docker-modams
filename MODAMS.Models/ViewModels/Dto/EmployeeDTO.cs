﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using MODAMS.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class EmployeeDTO
    {
        public Employee Employee { get; set; }


        [Display(Name = "EmployeeRole", ResourceType = typeof(EmployeeLabels))]
        [ValidateNever]
        public string roleId { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Employees { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> RoleList { get; set; }
    }
}
