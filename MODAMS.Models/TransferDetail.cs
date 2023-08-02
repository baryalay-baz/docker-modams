using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class TransferDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name ="Transfer Date")]
        public DateTime TransferDate { get; set; }

        [Display(Name ="Previous Store")]
        public int PrevStoreId { get; set; }

        [Required]
        [Display(Name = "Asset")]
        public int AssetId { get; set; }

        [Required]
        public int TransferId { get; set; }

        [ValidateNever]
        public Asset Asset { get; set; }

        [ValidateNever]
        public Transfer Transfer { get; set; }

    }
}
