using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class vwAlert
    {
        public int AssetId { get; set; }
        public string SubCategory { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AlertType { get; set; }= string.Empty;
        public int DepartmentId { get; set; }
        public string Department { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
    }
}
