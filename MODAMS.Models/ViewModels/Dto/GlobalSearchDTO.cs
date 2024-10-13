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
        public Asset Asset { get; set; } = new Asset();
        public AssetPicture AssetPicture { get; set; } = new AssetPicture();
    }
}
