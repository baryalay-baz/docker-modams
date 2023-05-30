using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class Donor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name="Donor Code")]
        public string Code { get; set; } = String.Empty;

        [Required]
        [Display(Name="Donor Name")]
        public string Name { get; set; } = String.Empty;
    }
}
