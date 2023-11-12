using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
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

        [AllowNull]
        public int? EmployeeId { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public string PrimaryKeyValue { get; set; }
        public string PropertyName { get; set; }

        [AllowNull]
        public string? OldValue { get; set; }
        [AllowNull]
        public string? NewValue { get; set; }

        [ValidateNever]
        public Employee Employee { get; set; }
    }
}
