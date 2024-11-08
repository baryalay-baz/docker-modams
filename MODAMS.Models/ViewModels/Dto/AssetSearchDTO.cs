using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
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
    }
}
