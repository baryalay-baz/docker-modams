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
    public class dtoAssetDocument
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
        public List<DocumentType> DocumentTypes { get; set; }

        [ValidateNever]
        public List<vwAssetDocuments> vwAssetDocuments { get; set; }


    }
}
