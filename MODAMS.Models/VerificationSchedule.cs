using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class VerificationSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EmployeeId { get; set; }
        public string VerificationStatus { get; set; } = "Pending";
        public string VerificationType { get; set; } = "Full Verification";

        [Required(ErrorMessage = "Number of assets to verify is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of assets to verify must be greater than 0.")]
        public int NumberOfAssetsToVerify { get; set; }
        public string Notes { get; set; } = String.Empty;

        [Required]
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
