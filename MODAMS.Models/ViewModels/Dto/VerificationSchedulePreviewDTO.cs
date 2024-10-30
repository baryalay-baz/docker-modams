using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class VerificationSchedulePreviewDTO
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EmployeeId { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public string VerificationType { get; set; } = string.Empty;
        public int NumberOfAssetsToVerify { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public IFormFile? file { get; set; }
        public bool IsAuthorized { get; set; }


        public VerificationRecord VerificationRecord { get; set; }

        public List<VerificationScheduleBarchartDTO> BarchartData { get; set; }

        public List<VerificationAssetsDTO> Assets { get; set; }

        [ValidateNever]
        public List<VerificationTeam> VerificationTeam { get; set; } // For adding a new team to the schedule

        [ValidateNever]
        public List<Employee> Employees { get; set; }

        [ValidateNever]
        public List<ProgressChartItemDTO> ProgressChart { get; set; }

    }
}
