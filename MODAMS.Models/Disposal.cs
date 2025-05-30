using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MODAMS.Localization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MODAMS.Models
{
    public class Disposal
    {
        [Key]
        public int Id { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "DisposalDate", ResourceType = typeof(DisposalLabels))]
        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime DisposalDate { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Asset", ResourceType = typeof(DisposalLabels))]
        public int AssetId { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "DisposalType", ResourceType = typeof(DisposalLabels))]
        public int DisposalTypeId { get; set; }


        [Display(Name = "DisposalNotes", ResourceType = typeof(DisposalLabels))]
        public string Notes { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Employee", ResourceType = typeof(DisposalLabels))]
        public int EmployeeId { get; set; }

        [Display(Name = "DisposalImage", ResourceType = typeof(DisposalLabels))]
        public string? ImageUrl { get; set; }

        [ValidateNever]
        public Asset Asset { get; set; }

        [ValidateNever]
        public DisposalType DisposalType { get; set; }

        [ValidateNever]
        public Employee Employee { get; set; }
    }
}
