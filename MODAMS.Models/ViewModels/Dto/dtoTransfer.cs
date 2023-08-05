using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoTransfer
    {
        public int StoreId { get; set; }
        public bool IsAuthorized { get; set; }

        public List<vwTransfer> vwTransfers = new List<vwTransfer>();

        [ValidateNever]
        public IEnumerable<SelectListItem> StoreList { get; set; }
    }

}
