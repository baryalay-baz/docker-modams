using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoTransfer
    {
        public int StoreId { get; set; }
        public bool IsAuthorized { get; set; }

        public List<vwTransfer> OutgoingTransfers = new List<vwTransfer>();
        public List<vwTransfer> IncomingTransfers = new List<vwTransfer>();

        [ValidateNever]
        public IEnumerable<SelectListItem> StoreList { get; set; }

    }

}
