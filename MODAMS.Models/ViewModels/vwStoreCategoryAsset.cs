﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    [Keyless]
    public class vwStoreCategoryAsset
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int StoreId { get; set; } 
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public int TotalAssets { get; set; }
        public decimal TotalCost { get; set; }
    }
}
