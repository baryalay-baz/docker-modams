using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoTransferPreview
    {
        [ValidateNever]
        public string? TransferBy { get; set; }
        
        [ValidateNever]
        public string? ReceivedBy { get; set; }

        [ValidateNever]
        public bool IsSender { get; set; }
        [ValidateNever]
        public bool IsReceiver { get; set; }
        
        public vwTransfer vwTransfer = new vwTransfer();
        public List<dtoTransferAsset> transferAssets = new List<dtoTransferAsset>();
    }
}
