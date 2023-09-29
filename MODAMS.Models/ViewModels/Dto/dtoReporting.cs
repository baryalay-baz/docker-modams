using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoReporting
    {
        [Display(Name ="Store")]
        public int StoreId { get; set; }

        [Display(Name ="Category")]
        public int CategoryId { get; set; } 

        [Display(Name ="Sub Category")]
        public int SubCategoryId { get; set; }

        [Display(Name = "Asset Status")]
        public int AssetStatusId { get; set; }



        [ValidateNever]
        public IEnumerable<SelectListItem> Stores { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> SubCategories { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Donors { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> AssetStatuses { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Conditions { get; set; }

    }
}
