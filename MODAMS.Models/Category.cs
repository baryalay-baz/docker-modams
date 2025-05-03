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

        [Required]
        [Display(Name ="Category Code")]
        public string CategoryCode { get; set; } = String.Empty;

        [Required]
        [Display(Name ="Category Name")]
        public string CategoryName { get; set; } = String.Empty;

        [Display(Name ="Category Name Somali")]
        public string CategoryNameSo { get; set; } = String.Empty;
    }
}
