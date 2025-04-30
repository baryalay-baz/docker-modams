using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace MODAMS.Models
{
    public class StoreEmployee
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Department")]
        public int StoreId { get; set; }
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }
        
        [ValidateNever]
        public Store Store { get; set; }
        [ValidateNever]
        public Employee Employee { get; set; }
    }
}
