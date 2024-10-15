using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class TransfersOutgoingAssetDTO
    {
        public DateTime? TransferDate { get; set; }
        public string StoreFrom { get; set; } = string.Empty;
        public string StoreTo { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public decimal Cost { get; set; } = decimal.Zero;
        public decimal CurrentValue { get; set; } = decimal.Zero;
    }
}
