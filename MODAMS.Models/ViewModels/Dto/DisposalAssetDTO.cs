using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class DisposalAssetDto
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string AssetName { get; set; }
        public string Identification { get; set; }
        public string? Barcode { get; set; }
    }
}
