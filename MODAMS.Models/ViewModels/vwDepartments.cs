using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class vwDepartments
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameSo { get; set; } = string.Empty;
        
        [Display(Name="Parent Department")]
        public int UpperLevelDeptId { get; set; }
        public string ULDeptName { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
