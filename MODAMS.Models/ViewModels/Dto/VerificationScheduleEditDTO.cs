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
    public class VerificationScheduleEditDTO
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int EmployeeId { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        [Required]
        public string VerificationType { get; set; } = string.Empty;
        public int NumberOfAssetsToVerify { get; set; }
        public string Notes { get; set; } = string.Empty;
        [Required]
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
