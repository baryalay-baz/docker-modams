using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class Asset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name="Asset Name")]
        public string Name { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Make")]
        public string Make { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Model")]
        public string Model { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Year")]
        public string Year { get; set; } = String.Empty;

        [Display(Name = "Manufacturing Country")]
        public string? ManufacturingCountry { get; set; }

        [Required]
        [Display(Name ="Serial Number")]
        public string SerialNo { get; set; } = String.Empty;

        [Display(Name ="Barcode")]
        public string? Barcode { get; set; }

        [Required]
        [Display(Name = "Engine Number")]
        public string Engine { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Chasis Number")]
        public string Chasis { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Plate Number")]
        public string Plate { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Specifications")]
        public string Specifications { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Initial Cost")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; } = 0;

        [Display(Name ="Purchase Date")]
        public DateTime? PurchaseDate { get; set; }

        [Display(Name ="Purchase Order Number")]
        public string? PONumber { get; set; }

        [Display(Name ="Reciept Date")]
        public DateTime? RecieptDate { get; set; }

        [Required]
        [Display(Name ="ProcuredBy")]
        public string? ProcuredBy { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; } = String.Empty;

        [Required]
        [Display(Name ="Sub Category")]
        public int SubCategoryId { get; set; }

        [Required]
        [Display(Name="Asset Condition")]
        public int ConditionId { get; set; }

        [Required]
        [Display(Name ="Store")]
        public int StoreId { get; set; }

        [Required]
        [Display(Name ="Donor")]
        public int? DonorId { get; set; }

        [Required]
        [Display(Name="Status")]
        public int AssetStatusId { get; set; }


        [ValidateNever]
        public SubCategory SubCategory { get; set; }
        [ValidateNever]
        public Store Store { get; set; }
        [ValidateNever]
        public Donor Donor { get; set;}
        [ValidateNever]
        public AssetStatus AssetStatus { get; set; }
        [ValidateNever]
        public Condition Condition { get; set; }

    }
}
