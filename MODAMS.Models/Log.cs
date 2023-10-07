using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }
        public int RecordId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public int ChangeType { get; set; }
        public DateTime Timestamp { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}
