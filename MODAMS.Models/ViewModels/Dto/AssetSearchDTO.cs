using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    [Flags]
    public enum AssetMatch
    {
        None = 0,
        Barcode = 1 << 0,
        Serial = 1 << 1,
        Plate = 1 << 2,
        Engine = 1 << 3,
        Chasis = 1 << 4,
        Name = 1 << 5,
        Make = 1 << 6,
        Model = 1 << 7,
        Sub = 1 << 8,
        Cat = 1 << 9
    }
    public class AssetSearchDTO
    {
        public int Id { get; set; }
        public string Category {  get; set; } = string.Empty;
        public string SubCategory {  get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Specifications { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string SerialNo { get; set; } = string.Empty;
        public string Engine{ get; set; } = string.Empty;
        public string Chasis { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public string AssetPicture { get; set; } = string.Empty;
        public bool IsVehicle { get; set; } = false;

        public int MatchExactMask { get; set; }
        public int MatchPrefixMask { get; set; }
        public int MatchContainsMask { get; set; }
    }
}
