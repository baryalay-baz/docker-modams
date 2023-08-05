using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoTransferPreview
    {
        public vwTransfer vwTransfer = new vwTransfer();
        public List<dtoTransferAsset> transferAssets = new List<dtoTransferAsset>();
    }
}
