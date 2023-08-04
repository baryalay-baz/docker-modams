using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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

        [Display(Name = "Transfer Date")]
        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Required]
        public DateTime? TransferDate { get; set; }

        [Required]
        [Display(Name ="Transfer to Store")]
        public int StoreId { get; set; }

        [Required]
        [Display(Name ="Transfer By")]
        public int EmployeeId { get; set; }

        [AllowNull]
        [Display(Name ="Submission for Acknowledgement")]
        public DateTime SubmissionForAcknowledgementDate { get; set;}

        [AllowNull]
        [Display(Name ="Acknowledgement Date")]
        public DateTime AcknowledgementDate { get; set; }

        [Display(Name = "Transfer Status")]
        public int TransferStatusId { get; set; }

        [Display(Name ="Notes")]
        public string Notes { get; set; } = string.Empty;

        [ValidateNever]
        public TransferStatus TransferStatus { get; set; }

        [ValidateNever]
        public Store Store { get; set; }
    }
}
