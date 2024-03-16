using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    [Keyless]
    public class vwTransferVoucher
    {
        public int Id { get; set; }
        public DateTime? TransferDate { get; set; }
        public string StoreFrom { get; set; }
        public string StoreTo { get; set; }
        public int NumberOfAssets { get; set; }
        public string Status { get; set; }
        public string SubCategoryName { get; set; }
        public int AssetId { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Name { get; set; }
        public string Identification { get; set; }
        public string ConditionName { get; set; }
        public string TransferNumber { get; set; }
        public int StoreFromId { get; set; }
        public int StoreToId { get; set; }
        public decimal Cost { get; set; }
        public string Barcode { get; set; }
        public string SerialNo { get; set; }
        public string SenderBarcode { get; set; }
        public string ReceiverBarcode { get; set; }
        public DateTime? SubmissionForAcknowledgementDate { get; set;}
        public DateTime? AcknowledgementDate { get; set; }
        public int TransferStatusId { get; set; }
        public int TransferId { get; set; }
    }
}
