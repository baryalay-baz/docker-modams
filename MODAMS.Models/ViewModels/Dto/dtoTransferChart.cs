using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoTransferChart
    {
        public int Id { get; set; }
        public string SubCategoryName { get; set; }
        public int StoreFromId { get; set; }
        public int StoreToId { get; set; }
        public int TotalAssets { get; set; }
    }
}
