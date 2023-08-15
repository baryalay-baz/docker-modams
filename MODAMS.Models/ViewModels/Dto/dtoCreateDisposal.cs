using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoCreateDisposal
    {
        public Disposal Disposal { get; set; } = new Disposal();
        public List<Asset> Assets { get; set; }
        public bool IsAuthorized { get; set; } = false;

        [ValidateNever]
        public IEnumerable<SelectListItem> DisposalTypeList { get; set; }

    }
}
