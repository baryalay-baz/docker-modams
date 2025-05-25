using MODAMS.Localization;
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


        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "DonorCode", ResourceType = typeof(DonorLabels))]
        public string Code { get; set; } = String.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "DonorName", ResourceType = typeof(DonorLabels))]
        public string Name { get; set; } = String.Empty;
    }
}
