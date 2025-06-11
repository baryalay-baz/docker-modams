using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;


namespace MODAMS.Models.ViewModels.Dto
{
    public class DepartmentHeadsDTO
    {
        [Display(Name = "Select Employee to Assign as Owner")]
        public int EmployeeId { get; set; }
        public int DepartmentId { get; set; }
        public int StoreId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public List<DepartmentHead> DepartmentHeads { get; set; } = new List<DepartmentHead>();
        public List<vwEmployees> DepartmentUsers { get; set; } = new List<vwEmployees>();

        [ValidateNever]
        public IEnumerable<SelectListItem> AvailableStoreOwners { get; set; } = new List<SelectListItem>();

        [ValidateNever]
        public IEnumerable<SelectListItem> AvailableUsers { get; set; } = new List<SelectListItem>();

        [ValidateNever]
        public IEnumerable<SelectListItem> Months { get; set; } = new List<SelectListItem>();

        [ValidateNever]
        public IEnumerable<SelectListItem> Years { get; set; } = new List<SelectListItem>();


    }
}
