using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public int EmployeeFrom { get; set; }
        public int EmployeeTo { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public int TargetSectionId { get; set; }
        public int TargetRecordId { get; set; }
        public bool IsViewed { get; set; } = false;


    }
}
