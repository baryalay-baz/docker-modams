using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class VerificationRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VerificationScheduleId { get; set; }

        [Required]
        public int AssetId { get; set; }

        [Required]
        public DateTime VerificationDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Required(ErrorMessage = "Please select verification result!")]
        public string Result { get; set; } = String.Empty;

        [Required(ErrorMessage = "Please provide verification comments!")]
        public string Comments { get; set; } = String.Empty;

        public string VerifiedBy { get; set; } = String.Empty;
        // Navigation properties
        public string ImageUrl { get; set; } = String.Empty;
        public VerificationSchedule VerificationSchedule { get; set; }
        public Asset Asset { get; set; }
    }
}
