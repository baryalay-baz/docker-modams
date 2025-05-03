using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class SubCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name="Sub-Category Code")]
        public string SubCategoryCode { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Sub-Category Name")]
        public string SubCategoryName { get; set; } = String.Empty;

        [Display(Name = "Sub-Category Name Somali")]
        public string SubCategoryNameSo { get; set; } = String.Empty;

        [Required]
        [Display(Name ="Life Span (Months)")]
        public int LifeSpan { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ValidateNever]
        public Category Category { get; set; }

    }
}
