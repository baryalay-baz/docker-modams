using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoNotification
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public int EmployeeFrom { get; set; }
        public int EmployeeTo { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public int TargetRecordId { get; set; }
        public bool IsViewed { get; set; } = false;
        public int NotificationSectionId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;

    }
}
