using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoDisposal
    {
        public List<Disposal> Disposals { get; set; }
        public bool IsAuthorized { get; set; }
        public int StoreId { get; set; }
    }
}
