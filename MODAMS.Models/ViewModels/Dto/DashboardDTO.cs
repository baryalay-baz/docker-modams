﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class DashboardDTO
    {
        public int StoreCount { get; set; }
        public int UserCount { get; set; }
        public decimal CurrentValue { get; set; }

        public List<vwCategoryAsset> CategoryAssets { get; set; } = new List<vwCategoryAsset>();
        public List<NewsFeed> NewsFeed { get; set; } = new List<NewsFeed>();
        public int TotalAssets()
        {
            int total = 0;
            if (CategoryAssets.Count > 0)
            {
                total = CategoryAssets.Sum(m => m.TotalAssets);
            }
            return total;
        }
        public decimal TotalCost() {
            decimal totalCost = 0;
            if(CategoryAssets.Count > 0)
            {
                totalCost = CategoryAssets.Sum(m => m.TotalCost);
            }
            return totalCost;
        }
    }
}
