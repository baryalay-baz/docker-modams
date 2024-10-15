using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace MODAMS.Models.ViewModels.Dto
{
    public class TransferPreviewDTO
    {
        [ValidateNever]
        public string? TransferBy { get; set; }
        
        [ValidateNever]
        public string? ReceivedBy { get; set; }

        [ValidateNever]
        public bool IsSender { get; set; }

        [ValidateNever]
        public bool IsReceiver { get; set; }

        [ValidateNever]
        public string FromSignature { get; set; } = string.Empty;

        [ValidateNever]
        public string ToSignature { get; set; } = string.Empty;
        
        public vwTransfer vwTransfer = new vwTransfer();
        public List<TransferAssetDTO> transferAssets = new List<TransferAssetDTO>();
    }
}
