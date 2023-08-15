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
    public class Disposal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Disposal Date")]
        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime DisposalDate { get; set; }

        [Required]
        [Display(Name = "Asset")]
        public int AssetId { get; set; }

        [Required]
        [Display(Name = "Disposal Type")]
        public int DisposalTypeId { get; set; }


        [Display(Name ="Disposal Notes")]
        public string Notes { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Display(Name ="Disposal Image")]
        public string ImageUrl { get; set; }

        [ValidateNever]
        public Asset Asset { get; set; }

        [ValidateNever]
        public DisposalType DisposalType { get; set; }

        [ValidateNever]
        public Employee Employee { get; set; }
    }
}
