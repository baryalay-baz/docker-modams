using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoTransferAsset
    {
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
    }
}
