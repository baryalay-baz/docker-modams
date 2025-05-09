using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using MODAMS.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class AssetDocumentDTO
    {
        [Key]
        public int Id { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "DocumentName", ResourceType = typeof(AssetDocumentLabels))]
        public string Name { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "DocumentUrl", ResourceType = typeof(AssetDocumentLabels))]
        public string DocumentUrl { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "AssetName", ResourceType = typeof(AssetDocumentLabels))]
        public int AssetId { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "DocumentType", ResourceType = typeof(AssetDocumentLabels))]
        public int DocumentTypeId { get; set; }

        [ValidateNever]
        public List<vwAssetDocument> DocumentList { get; set; }

        [ValidateNever]
        public string AssetInfo { get; set; } = string.Empty;

        [ValidateNever]
        public int StoreId { get; set; }

        [ValidateNever]
        public string StoreName { get; set; } = string.Empty;

    }
}
