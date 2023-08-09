using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class AssetHistory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int AssetId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime TimeStamp { get; set; }

        [Required]
        public int TransactionRecordId { get; set; }

        [Required]
        public int TransactionTypeId { get; set; }

        [ValidateNever]
        public TransactionType TransactionType { get; set; }
    }
}
