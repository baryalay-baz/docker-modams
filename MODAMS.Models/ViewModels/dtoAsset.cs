using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MODAMS.Models.ViewModels
{
    public class dtoAsset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Asset Name")]
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
        [Display(Name = "Serial Number")]
        public string SerialNo { get; set; } = String.Empty;

        [Display(Name = "Barcode")]
        public string? Barcode { get; set; }

        [Required]
        [Display(Name = "Engine Number")]
        public string Engine { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Chasis Number")]
        public string Chasis { get; set; } = String.Empty;

        [Required]
        [Display(Name = "License Plate Number")]
        public string Plate { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Specifications")]
        public string Specifications { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Initial Cost")]
        public decimal Cost { get; set; } = 0;

        [Required]
        [Display(Name = "Purchase Date")]
        [Column(TypeName ="date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? PurchaseDate { get; set; }

        [Required]
        [Display(Name = "Purchase Order Number")]
        public string? PONumber { get; set; }

        [Required]
        [Display(Name = "Reciept Date")]
        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? RecieptDate { get; set; }

        [Required]
        [Display(Name = "Procured By")]
        public string? ProcuredBy { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; } = String.Empty;

        [Required]
        [Display(Name="Category")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }

        [Required]
        [Display(Name = "Asset Condition")]
        public int ConditionId { get; set; }

        [Required]
        [Display(Name = "Store")]
        public int StoreId { get; set; }

        [Required]
        [Display(Name = "Donor")]
        public int? DonorId { get; set; }

        [Required]
        [Display(Name = "Status")]
        public int AssetStatusId { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> SubCategories { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Donors { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Statuses { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Conditions { get; set; }
        
    }
}
