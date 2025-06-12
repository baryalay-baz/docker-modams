using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class DisposalPreviewDTO
    {
        public int Id { get; set; }
        public string DisposalType { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string SubCategoryName { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string Identification { get; set; } = string.Empty;
        public DateTime DisposalDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string DisposalNotes { get; set; } = string.Empty;
    }
}
