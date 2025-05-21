using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MODAMS.Localization;

namespace MODAMS.Models
{
    public class VerificationSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        [Display(Name = "StartDate", ResourceType = typeof(VerificationScheduleLabels))]
        public DateTime StartDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(VerificationScheduleLabels))]
        public DateTime EndDate { get; set; }
        
        public int EmployeeId { get; set; }

        [Display(Name = "VerificationStatus", ResourceType = typeof(VerificationScheduleLabels))]
        public string VerificationStatus { get; set; } = "Pending";

        [Display(Name = "VerificationType", ResourceType = typeof(VerificationScheduleLabels))]
        public string VerificationType { get; set; } = "Full Verification";

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "NumberOfAssetsRequired")]
        [Range(1, int.MaxValue,
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "NumberOfAssetsGreaterThanZero")]

        [Display(Name = "NumberOfAssetsToVerify", ResourceType = typeof(VerificationScheduleLabels))]
        public int NumberOfAssetsToVerify { get; set; }

        [Display(Name = "VerificationNotes", ResourceType = typeof(VerificationScheduleLabels))]
        public string Notes { get; set; } = String.Empty;

        [Required(
            ErrorMessageResourceType = typeof(ValidationMessages),
            ErrorMessageResourceName = "Required")]
        public int StoreId { get; set; }

        [ValidateNever]
        public Store Store { get; set; }
        [ValidateNever]
        public ICollection<VerificationRecord> VerificationRecords { get; set; }

        [ValidateNever]
        [JsonIgnore]
        public ICollection<VerificationTeam> VerificationTeams { get; set; }
    }
}
