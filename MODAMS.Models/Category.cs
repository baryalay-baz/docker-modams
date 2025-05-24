using MODAMS.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "CategoryCode", ResourceType = typeof(CategoryLabels))]
        public string CategoryCode { get; set; } = String.Empty;
        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "CategoryName", ResourceType = typeof(CategoryLabels))]
        public string CategoryName { get; set; } = String.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "CategoryNameSomali", ResourceType = typeof(CategoryLabels))]
        public string CategoryNameSo { get; set; } = String.Empty;
    }
}
