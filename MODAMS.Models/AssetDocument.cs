﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class AssetDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name="Document Name")]
        public string Name { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Document Url")]
        public string DocumentUrl { get; set; } = String.Empty;

        [Required]
        [Display(Name ="Asset")]
        public int AssetId { get; set; }

        [Required]
        [Display(Name ="Document Type")]
        public int DocumentTypeId { get; set; }

        [ValidateNever]
        public Asset Asset { get; set; }

        [ValidateNever]
        public DocumentType DocumentType { get; set; }
    }
}
