using Microsoft.AspNetCore.Mvc.Rendering;

namespace MODAMS.Models.ViewModels.Dto.Contracts
{
    public interface IAssetDtoLists
    {
        IEnumerable<SelectListItem> Categories { get; set; }
        IEnumerable<SelectListItem> SubCategories { get; set; }
        IEnumerable<SelectListItem> Donors { get; set; }
        IEnumerable<SelectListItem> Statuses { get; set; }
        IEnumerable<SelectListItem> Conditions { get; set; }
    }
}
