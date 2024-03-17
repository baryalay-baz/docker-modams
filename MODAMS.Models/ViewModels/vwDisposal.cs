using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class vwDisposal
    {
        public int Id { get; set; }
        public DateTime? DisposalDate { get; set; }
        public string Type { get; set; }
        public string SubCategoryName { get; set; }
        public string Name { get; set; }
        public string DepartmentName { get; set; }
        public string Identification {  get; set; }
        public decimal Cost { get; set; }
        public int StoreId { get; set; }
        public int DisposalTypeId { get; set; }

    }
}
