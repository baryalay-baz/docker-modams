using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoDashboard
    {
        public List<vwCategoryAsset> CategoryAssets { get; set; } = new List<vwCategoryAsset>();

        public int TotalAssets()
        {
            int total = 0;
            if (CategoryAssets.Count > 0)
            {
                total = CategoryAssets.Sum(m => m.TotalAssets);
            }
            return total;
        }
    }
}
