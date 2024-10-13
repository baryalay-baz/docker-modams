using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class vwAssetDocument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DocumentUrl { get; set; }
        public int AssetId { get; set; }
        public int DocumentTypeId { get; set; }
    }
}
