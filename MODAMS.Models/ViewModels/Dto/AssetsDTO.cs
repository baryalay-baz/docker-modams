using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class AssetsDTO
    {
        public List<Asset> assets { get; set; }
        public List<vwStoreCategoryAsset> categories { get; set; }

        [ValidateNever]
        public int SubCategoryId { get; set; }

        [ValidateNever]
        public string SubCategoryName { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> CategorySelectList { get; set; } = Enumerable.Empty<SelectListItem>();

        public int StoreOwnerId { get; set; }
        public int TotalAssets()
        {
            return assets.Count;
        }
        public bool IsAuthorized { get; set; } = false;

        [ValidateNever]
        public int StoreId { get; set; }

        [ValidateNever]
        public string StoreName { get; set; }

        [ValidateNever]
        public string StoreOwnerInfo { get; set; }

    }
}
