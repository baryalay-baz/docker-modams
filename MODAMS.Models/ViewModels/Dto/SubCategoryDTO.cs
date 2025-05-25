using MODAMS.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class SubCategoryDTO
    {
        public int Id { get; set; }
        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "SubCategoryCode", ResourceType = typeof(SubcategoryLabels))]
        public string SubCategoryCode { get; set; } = string.Empty;
        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "SubCategoryName", ResourceType = typeof(SubcategoryLabels))]
        public string SubCategoryName { get; set; } = string.Empty;
        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "SubCategoryNameSo", ResourceType = typeof(SubcategoryLabels))]
        public string SubCategoryNameSo { get; set; } = string.Empty;
        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "Lifespan", ResourceType = typeof(SubcategoryLabels))]
        public int LifeSpan { get; set; }
        public string CategoryCode { get; set; } = string.Empty;
        [Display(Name = "CategoryName", ResourceType = typeof(CategoryLabels))]
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
}
