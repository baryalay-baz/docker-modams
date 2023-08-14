using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public DateTime DisposalDate { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        public int DisposalTypeId { get; set; }

        [Display(Name ="Disposal Notes")]
        public string Notes { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ValidateNever]
        public Asset Asset { get; set; }

        [ValidateNever]
        public DisposalType DisposalType { get; set; }

        [ValidateNever]
        public Employee Employee { get; set; }
    }
}
