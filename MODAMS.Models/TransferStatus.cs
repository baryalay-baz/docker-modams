using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class TransferStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name ="Transfer Status")]
        public string Status { get; set; }

        [Display(Name = "Transfer Status Somali")]
        public string StatusSo { get; set; } = String.Empty;
    }
}
