using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoAssetList
    {
        public int CategoryId { get; set; }
        public List<Asset> AssetList { get; set; } = new List<Asset>();

        [ValidateNever]
        public IEnumerable<SelectListItem> CategorySelectList { get; set; } = Enumerable.Empty<SelectListItem>();

        public int TotalAssets()
        {
            return AssetList.Count;
        }

    }
}
