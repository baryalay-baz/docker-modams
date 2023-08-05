using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoCreateTransfer
    {
        public Transfer Transfer { get; set; } = new Transfer();
        public List<dtoTransferAsset> Assets { get; set; }
        
        [ValidateNever]
        public IEnumerable<SelectListItem> StoreList { get; set; }
    }
}
