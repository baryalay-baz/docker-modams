using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MODAMS.Localization;


namespace MODAMS.Models.ViewModels.Dto
{
    public class AssetEditDTO
    {
        [Key]
        public int Id { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [RegularExpression(@"^[a-zA-Z0-9 -]*$",
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "InvalidPattern")]
        [Display(Name = "AssetName", ResourceType = typeof(CreateAssetLabels))]
        public string Name { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [RegularExpression(@"^[a-zA-Z0-9 -]*$",
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "InvalidPattern")]
        [Display(Name = "Make", ResourceType = typeof(CreateAssetLabels))]
        public string Make { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [RegularExpression(@"^[a-zA-Z0-9 -]*$",
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "InvalidPattern")]
        [Display(Name = "Model", ResourceType = typeof(CreateAssetLabels))]
        public string Model { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Year", ResourceType = typeof(CreateAssetLabels))]
        public string Year { get; set; } = string.Empty;

        [Display(Name = "ManufacturingCountry", ResourceType = typeof(CreateAssetLabels))]
        public string? ManufacturingCountry { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "SerialNo", ResourceType = typeof(CreateAssetLabels))]
        public string SerialNo { get; set; } = string.Empty;

        [Display(Name = "Barcode", ResourceType = typeof(CreateAssetLabels))]
        public string? Barcode { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Engine", ResourceType = typeof(CreateAssetLabels))]
        public string Engine { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Chasis", ResourceType = typeof(CreateAssetLabels))]
        public string Chasis { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Plate", ResourceType = typeof(CreateAssetLabels))]
        public string Plate { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Specifications", ResourceType = typeof(CreateAssetLabels))]
        public string Specifications { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Cost", ResourceType = typeof(CreateAssetLabels))]
        public decimal Cost { get; set; } = 0;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "PurchaseDate", ResourceType = typeof(CreateAssetLabels))]
        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? PurchaseDate { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "PONumber", ResourceType = typeof(CreateAssetLabels))]
        public string? PONumber { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "RecieptDate", ResourceType = typeof(CreateAssetLabels))]
        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? RecieptDate { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "ProcuredBy", ResourceType = typeof(CreateAssetLabels))]
        public string? ProcuredBy { get; set; }

        [Display(Name = "Remarks", ResourceType = typeof(CreateAssetLabels))]
        public string? Remarks { get; set; } = string.Empty;
        [Required(
                    ErrorMessageResourceType = typeof(ValidationMessages),
                    ErrorMessageResourceName = "Required")]
        [Display(Name = "Category", ResourceType = typeof(CreateAssetLabels))]
        public int CategoryId { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "SubCategory", ResourceType = typeof(CreateAssetLabels))]
        public int SubCategoryId { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "AssetCondition", ResourceType = typeof(CreateAssetLabels))]
        public int ConditionId { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Store", ResourceType = typeof(CreateAssetLabels))]
        public int StoreId { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Donor", ResourceType = typeof(CreateAssetLabels))]
        public int DonorId { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Status", ResourceType = typeof(CreateAssetLabels))]
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

        [ValidateNever]
        public string StoreName { get; set; }

        [ValidateNever]
        public bool IsAuthorized { get; set; } = false;

    }
}
