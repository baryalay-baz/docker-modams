using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MODAMS.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class Transfer
    {
        [Key]
        public int Id { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "TransferDate", ResourceType = typeof(TransferLabels))]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        public DateTime? TransferDate { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "TransferFrom", ResourceType = typeof(TransferLabels))]
        public int StoreFromId { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "TransferTo", ResourceType = typeof(TransferLabels))]
        public int StoreId { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "TransferBy", ResourceType = typeof(TransferLabels))]
        public int EmployeeId { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "TransferNumber", ResourceType = typeof(TransferLabels))]
        public string TransferNumber { get; set; } = string.Empty;

        [AllowNull]
        [Display(Name = "SubmissionForAcknowledgementDate", ResourceType = typeof(TransferLabels))]
        public DateTime SubmissionForAcknowledgementDate { get; set;}

        [AllowNull]
        [Display(Name = "AcknowledgementDate", ResourceType = typeof(TransferLabels))]
        public DateTime AcknowledgementDate { get; set; }
        
        [Display(Name = "TransferStatus", ResourceType = typeof(TransferLabels))]
        public int TransferStatusId { get; set; }

        [Display(Name = "TransferNotes", ResourceType = typeof(TransferLabels))]
        public string? Notes { get; set; }

        public string SenderBarcode { get; set; } = string.Empty;
        public string ReceiverBarcode { get; set; } = string.Empty;


        [ValidateNever]
        public TransferStatus TransferStatus { get; set; }

        [ValidateNever]
        public Store Store { get; set; }
    }
}
