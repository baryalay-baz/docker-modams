using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace MODAMS.Models.ViewModels.Dto
{
    public class TransferCreateDTO
    {
        public Transfer Transfer { get; set; } = new Transfer();
        public List<TransferAssetDTO> Assets { get; set; }

        [ValidateNever]
        public string StoreFrom { get; set; } = string.Empty;

        [ValidateNever]
        public IEnumerable<SelectListItem> StoreList { get; set; }
    }
}
