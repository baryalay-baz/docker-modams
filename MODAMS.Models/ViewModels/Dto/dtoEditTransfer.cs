﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoEditTransfer
    {
        public Transfer Transfer { get; set; } = new Transfer();
        //public List<TransferDetail> transferDetails { get; set; }
        public List<dtoTransferAsset> Assets { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> StoreList { get; set; }
    }
}