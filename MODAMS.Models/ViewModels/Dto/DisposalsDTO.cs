using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class DisposalsDTO
    {
        public List<Disposal> Disposals { get; set; }
        public bool IsAuthorized { get; set; }
        public int StoreId { get; set; }
        public List<DisposalChart> ChartData { get; set; }
    }

    public class DisposalChart{ 
        public string Type { get; set; }
        public int EmployeeId { get; set; }
        public int Count { get; set; }
    }
}
