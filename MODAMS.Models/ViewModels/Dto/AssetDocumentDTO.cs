using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        [Required]
        [Display(Name = "Document Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Document Url")]
        public string DocumentUrl { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Asset")]
        public int AssetId { get; set; }

        [Required]
        [Display(Name = "Document Type")]
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
