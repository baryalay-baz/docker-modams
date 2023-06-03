using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class AssetDocumentChecklist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Document Name")]
        public string Name { get; set; } = String.Empty;

        [Required]
        [Display(Name ="Is Required")]
        public bool IsRequired { get; set; }
    }
}
