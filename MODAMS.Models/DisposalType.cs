using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class DisposalType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name ="Disposal Type")]
        public string Type { get; set; }

        [Display(Name = "Disposal Type Somali")]
        public string TypeSo { get; set; } = String.Empty;
    }
}
