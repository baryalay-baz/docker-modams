using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using MODAMS.Localization;

namespace MODAMS.Models.ViewModels.Dto
{
    public class ReportingDTO
    {
        public string ReportId { get; set; }

        //General DateFrom and DateTO

        [Display(Name = "DateFrom", ResourceType = typeof(Reporting))]
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "DateTo", ResourceType = typeof(Reporting))]
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }


        //Section for Asset Report
        [Display(Name = "Store", ResourceType = typeof(Reporting))]
        public int AssetStoreId { get; set; }

        [Display(Name = "Category", ResourceType = typeof(Reporting))]
        public int CategoryId { get; set; }

        [Display(Name = "SubCategory", ResourceType = typeof(Reporting))]
        public int SubCategoryId { get; set; }

        [Display(Name = "AssetStatus", ResourceType = typeof(Reporting))]
        public int AssetStatusId { get; set; }

        [Display(Name = "Condition", ResourceType = typeof(Reporting))]
        public int AssetConditionId { get; set; }

        [Display(Name = "Donor", ResourceType = typeof(Reporting))]
        public int DonorId { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> AssetStores { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> Categories { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> SubCategories { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> Donors { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> AssetStatuses { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> Conditions { get; set; }



        //Section for TransferReport
        [Display(Name = "TransferId", ResourceType = typeof(Reporting))]
        public int TransferId { get; set; }

        [Display(Name = "StoreFrom", ResourceType = typeof(Reporting))]
        public int StoreFromId { get; set; }

        [Display(Name = "StoreTo", ResourceType = typeof(Reporting))]
        public int StoreToId { get; set; }

        [Display(Name = "TransferStatus", ResourceType = typeof(Reporting))]
        public int TransferStatusId { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> TransferStatuses { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> TransferStores { get; set; }

        //Section for DisposalReport
        [Display(Name = "Store", ResourceType = typeof(Reporting))]
        public int DisposalStoreId { get; set; }

        [Display(Name = "DisposalType", ResourceType = typeof(Reporting))]
        public int DisposalTypeId { get; set; }
        [ValidateNever] public IEnumerable<SelectListItem> DisposalTypes { get; set; }

        private static DateTime TodayInMogadishu()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi"); // Somalia uses EAT; Nairobi ID is widely available
                                                                            // If "Africa/Mogadishu" exists in your container, prefer it:
                                                                            // var tz = TimeZoneInfo.FindSystemTimeZoneById("Africa/Mogadishu");
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz).Date;
        }
    }
}
