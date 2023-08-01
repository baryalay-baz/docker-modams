using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoAlerts
    {
        public int DepartmentId { get; set; }
        public List<vwAlert> Alerts { get; set; } = new List<vwAlert>();

        [ValidateNever]
        public IEnumerable<SelectListItem> DepartmentList { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
