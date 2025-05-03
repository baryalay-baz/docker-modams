using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class AssetStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name ="Asset Status")]
        public string StatusName { get; set; }

        [Display(Name = "Asset Status Somali")]
        public string StatusNameSo { get; set; } = String.Empty;

    }
}
