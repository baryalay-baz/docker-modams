using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class StoreUsersDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public int SupervisorEmployeeId { get; set; }
        public string ImageUrl { get; set; } = "/assets/images/faces/profile_placeholder.png";
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int StoreId { get; set; }
        public int DepartmentId { get; set; }
    }
}
