﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class dtoAssetInfo
    {
        public Asset Asset { get; set; }
        public List<AssetDocument> Documents { get; set; }
        public dtoAssetPictures dtoAssetPictures { get; set; }
    }
}