﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class dtoAssets
    {
        public List<Asset> assets { get; set; }
        public List<vwStoreCategoryAsset> categories { get; set; }
        public int StoreOwnerId { get; set; }
        public int TotalAssets()
        {
            return assets.Count;
        }
        public bool IsAuthorized { get; set; } = false;
    }
}