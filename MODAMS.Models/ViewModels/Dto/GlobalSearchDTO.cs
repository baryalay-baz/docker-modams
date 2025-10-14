using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class GlobalSearchDTO
    {
        public int TransferId {  get; set; }
        public List<AssetSearchDTO> Assets { get; set; } = new List<AssetSearchDTO>();
    }
}
