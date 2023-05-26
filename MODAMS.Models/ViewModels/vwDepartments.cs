using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class vwDepartments
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UpperLevelDeptId { get; set; }
        public string ULDeptName { get; set; }
        public int EmployeeId { get; set; }
        public string OwnerName { get; set; }
    }
}
