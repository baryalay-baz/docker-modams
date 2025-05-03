using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MODAMS.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Department Name Somali")]
        public string NameSo { get; set; } = string.Empty;

        [Required]
        [Display(Name ="Upper-level Department")]
        public int UpperLevelDeptId { get; set; }

        [AllowNull]
        [Display(Name ="Department Owner")]
        public int? EmployeeId { get; set; }

    }
}
