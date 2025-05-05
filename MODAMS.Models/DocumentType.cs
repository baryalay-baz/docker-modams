using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class DocumentType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name="Document Type")]
        public string Name { get; set; } = String.Empty;
        public string NameSo { get; set; } = String.Empty;
        [Required]
        [Display(Name="Is Required")]
        public bool IsRequired { get; set; }

        

    }
}
