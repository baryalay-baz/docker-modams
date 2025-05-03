using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class Condition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name ="Asset Condition")]
        public string ConditionName { get; set; } = String.Empty;

        [Display(Name = "Asset Condition Somali")]
        public string ConditionNameSo { get; set; } = String.Empty;
    }
}
