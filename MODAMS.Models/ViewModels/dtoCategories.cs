﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class dtoCategories
    {
        public int? SelectedCategoryId { get; set; }
        public List<Category> categories { get; set; }
        public List<SubCategory> subCategories { get; set; }
    }
}