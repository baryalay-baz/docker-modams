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
    public class DepartmentsDTO
    {
        public List<vwDepartments> departments { get; set; } = new List<vwDepartments>();
        public List<StoreUsersDTO> storeUsers { get; set; } = new List<StoreUsersDTO>();

    }
}
