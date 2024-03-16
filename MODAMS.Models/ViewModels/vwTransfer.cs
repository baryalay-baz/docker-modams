using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class vwTransfer
    {
        public int Id { get; set; }
        public DateTime? TransferDate { get; set; }
        public int StoreId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? SubmissionforAcknowledgementDate { get; set; }
        public DateTime? AcknowledgementDate { get; set; }
        public int TransferStatusId { get; set; }
        public string Notes { get; set; }
        public string TransferNumber { get; set; }
        public string Status { get; set; }
        public string StoreTo { get; set; }
        public int NumberOfAssets { get; set; }
        public int StoreFromId { get; set; }
        public string StoreFrom { get; set; }
        public string SenderBarcode { get; set; }
        public string ReceiverBarcode { get; set; }
    }
}
