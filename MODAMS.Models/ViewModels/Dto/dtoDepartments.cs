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
    public class dtoDepartments
    {
        public List<vwDepartments> departments { get; set; }
        public List<vwEmployees> employees { get; set; }

        public List<vwEmployees> departmentUsers(int employeeId) {
            var du = new List<vwEmployees>();

            du = employees.Where(m => m.SupervisorEmployeeId == employeeId).ToList();

            return du;
        }
    }
}
