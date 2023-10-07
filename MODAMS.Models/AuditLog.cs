using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int EmployeeId { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public string PrimaryKeyValue { get; set; }
        public string PropertyName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
