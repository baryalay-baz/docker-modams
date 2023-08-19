using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoEditDisposal
    {
        public bool IsAuthorized { get; set; } = false;
        public Disposal Disposal { get; set; } = new Disposal();
        public IFormFile? file { get; set; }


        //Elements not to be validated
        [ValidateNever]
        public string StoreName { get; set; }

        [ValidateNever]
        public string StoreOwner { get; set; }

        [ValidateNever]
        public List<Asset> Assets { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> DisposalTypeList { get; set; }

        [ValidateNever]
        public Asset CurrentDisposedAsset { get; set; }
    }
}
