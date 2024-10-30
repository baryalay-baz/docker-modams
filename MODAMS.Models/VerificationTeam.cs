using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class VerificationTeam
    {
        public int Id { get; set; }

        [Required]
        public int VerificationScheduleId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        public string Role { get; set; } = String.Empty;

        // Navigation properties
        public VerificationSchedule VerificationSchedule { get; set; }
        public Employee Employee { get; set; }
    }
}
