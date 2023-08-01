using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class vwCategoryAsset
    {
        public int Id { get; set; } 
        public string CategoryName { get; set; } = string.Empty;
        public int TotalAssets { get; set; }
        public decimal TotalCost { get; set; }
    }
}
