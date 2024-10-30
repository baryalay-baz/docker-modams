using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class VerificationTeamEditDTO
    {
        public int Id { get; set; }

        [Required]
        public int VerificationScheduleId { get; set; } 

        [Required]
        public int EmployeeId { get; set; } 

        public string Role { get; set; } = String.Empty;
    }
}
