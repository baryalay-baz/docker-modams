using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using MODAMS.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class VerificationScheduleEditDTO
    {
        public int Id { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "StartDate", ResourceType = typeof(VerificationScheduleLabels))]
        public DateTime StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(VerificationScheduleLabels))]
        public DateTime? EndDate { get; set; }
        public int EmployeeId { get; set; }

        [Display(Name = "VerificationStatus", ResourceType = typeof(VerificationScheduleLabels))]
        public string VerificationStatus { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "VerificationType", ResourceType = typeof(VerificationScheduleLabels))]
        public string VerificationType { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "NumberOfAssetsRequired")]
        [Range(1, int.MaxValue,
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "NumberOfAssetsGreaterThanZero")]

        [Display(Name = "NumberOfAssetsToVerify", ResourceType = typeof(VerificationScheduleLabels))]
        public int NumberOfAssetsToVerify { get; set; }

        [Display(Name = "VerificationNotes", ResourceType = typeof(VerificationScheduleLabels))]
        public string Notes { get; set; } = string.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        public int StoreId { get; set; }

        [ValidateNever]
        public List<VerificationTeamEditDTO> EditTeam { get; set; } // For adding a new team to the schedule

        [ValidateNever]
        public IEnumerable<SelectListItem> EmployeesList { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> StoreList { get; set; }

        [ValidateNever]
        public List<Employee> Employees { get; set; }

        [ValidateNever]
        public List<Store> Stores { get; set; }

        public int AssetCount(int storeId)
        {
            var store = Stores.FirstOrDefault(ps => ps.Id == storeId);
            return store?.Assets?.Count ?? 0;
        }
    }
}
