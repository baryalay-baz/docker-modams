

namespace MODAMS.Models.ViewModels.Dto
{
    public class TransferAssetDTO
    {
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Cost { get; set; } = decimal.Zero;
        public decimal CurrentValue { get; set; } = decimal.Zero;
        public bool IsSelected { get; set; } = false;
        public int TransferId { get; set; } = 0;
    }
}
