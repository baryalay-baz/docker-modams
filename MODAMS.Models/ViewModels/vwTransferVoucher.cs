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
        public string StoreFrom { get; set; } = string.Empty;
        public string StoreTo { get; set; } = string.Empty;
        public int NumberOfAssets { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SubCategoryName { get; set; } = string.Empty;
        public int AssetId { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Identification { get; set; } = string.Empty;
        public string ConditionName { get; set; } = string.Empty;
        public string TransferNumber { get; set; } = string.Empty;
        public int StoreFromId { get; set; }
        public int StoreToId { get; set; }
        public decimal Cost { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string SerialNo { get; set; } = string.Empty;
        public string SenderBarcode { get; set; } = string.Empty;
        public string ReceiverBarcode { get; set; } = string.Empty;
        public DateTime? SubmissionForAcknowledgementDate { get; set;}
        public DateTime? AcknowledgementDate { get; set; }
        public int TransferStatusId { get; set; }
        public int TransferId { get; set; }
        public string StoreFromSo { get; set; } = string.Empty;
        public string StoreToSo { get; set; } = string.Empty;
        public string SubCategoryNameSo { get; set; } = string.Empty;
        public string StatusSo { get; set; } = string.Empty;
        public string ConditionNameSo { get; set; } = string.Empty;

    }
}
