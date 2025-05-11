using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class vwStore
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameSo { get; set; }
        public string Description { get; set; }
        public int DepartmentId { get; set; }
        public int UpperLevelDeptId { get; set; }
        public int? EmployeeId { get; set; }
        public string StoreOwner { get; set; }
        public int? StoreType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DepCost { get; set; }

        public int? TotalCount { get; set; }
    }
}
