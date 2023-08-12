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
        
        public List<dtoTransferChart> IncomingChartData = new List<dtoTransferChart>();
        public List<dtoTransferChart> OutgoingChartData = new List<dtoTransferChart>();

        [ValidateNever]
        public IEnumerable<SelectListItem> StoreList { get; set; }

    }

}
