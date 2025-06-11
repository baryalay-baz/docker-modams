using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MODAMS.Models
{
    public class Store
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name="Store Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Store Name Somali")]
        public string NameSo { get; set; } = string.Empty;

        [Display(Name ="Description")]
        public string Description { get; set; } = String.Empty;

        [Required]
        [Display(Name ="Department")]
        public int DepartmentId { get; set; }

        [ValidateNever]
        public Department Department { get; set; }


        //Sub tables
        [ValidateNever]
        public ICollection<VerificationSchedule> VerificationSchedules { get; set; }
        [ValidateNever]
        public ICollection<Asset> Assets { get; set; }
        [ValidateNever]
        public ICollection<StoreEmployee> StoreEmployees { get; set; }

    }
}
