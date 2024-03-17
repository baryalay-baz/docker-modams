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
        public string ReportId { get; set; }


        //Section for Asset Report
        [Display(Name = "Store")]
        public int AssetStoreId { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }

        [Display(Name = "Asset Status")]
        public int AssetStatusId { get; set; }

        [Display(Name = "Asset Condition")]
        public int AssetConditionId { get; set; }

        [Display(Name = "Donor")]
        public int DonorId { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> AssetStores { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> Categories { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> SubCategories { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> Donors { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> AssetStatuses { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> Conditions { get; set; }



        //Section for TransferReport
        [Display(Name = "Transfer Id")]
        public int TransferId { get; set; }

        [Display(Name = "Transfer From Store")]
        public int StoreFromId { get; set; }

        [Display(Name = "Transfer To Store")]
        public int StoreToId { get; set; }

        [Display(Name = "Transfer Status")]
        public int TransferStatusId { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> TransferStatuses { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> TransferStores { get; set; }

        //Section for DisposalReport
        [Display(Name ="Store")]
        public int DisposalStoreId { get; set; }

        [Display(Name = "Disposal Type")]
        public int DisposalTypeId { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> DisposalTypes { get; set; }
    }
}
