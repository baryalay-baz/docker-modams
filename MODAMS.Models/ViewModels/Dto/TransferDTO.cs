using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MODAMS.Models.ViewModels.Dto
{
    public class TransferDTO
    {

        public int StoreId { get; set; }
        public bool IsAuthorized { get; set; }

        [ValidateNever]
        public int TransferStatus { get; set; }

        public List<vwTransfer> OutgoingTransfers = new List<vwTransfer>();
        public List<vwTransfer> IncomingTransfers = new List<vwTransfer>();

        public List<TransferChartDTO> IncomingChartData = new List<TransferChartDTO>();
        public List<TransferChartDTO> OutgoingChartData = new List<TransferChartDTO>();

        [ValidateNever]
        public IEnumerable<SelectListItem> StoreList { get; set; }


        public int TotalTransferCount(int storeId)
        {
            var outgoingTransfers = OutgoingTransfers.Where(m => m.TransferStatusId == 3 && m.StoreFromId == storeId).ToList();
            var totalAssets = outgoingTransfers.Sum(m => m.NumberOfAssets);

            return totalAssets;
        }
        public int TotalReceivedCount(int storeId)
        {
            var incomingTransfers = IncomingTransfers.Where(m => m.TransferStatusId == 3 && m.StoreId == storeId).ToList();
            var totalAssets = incomingTransfers.Sum(m => m.NumberOfAssets);

            return totalAssets;
        }

        public decimal TotalTransferValue { get; set; }
        public decimal TotalReceivedValue { get; set; }

    }

}
