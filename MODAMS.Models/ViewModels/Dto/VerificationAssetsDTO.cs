using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class VerificationAssetsDTO
    {
        public int Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SerialNo { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public string VerifiedBy { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
        public string Result { get; set; } = string.Empty;
        public int VerificationRecordId { get; set; }
    }
}
