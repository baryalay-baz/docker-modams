using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class vwStores
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DepartmentId { get; set; }
        public int UpperLevelDeptId { get; set; }
        public int? EmployeeId { get; set; }
        public string StoreOwner { get; set; }
        public int? StoreType { get; set; }
        public decimal? TotalCost { get; set; }
        public int? TotalCount { get; set; }
    }
}
