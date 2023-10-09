using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class NewsFeed
    {
        [Key]
        public int Id { get; set; }
        public string Description{ get; set; }
        public DateTime TimeStamp { get; set; }
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action {  get; set; } 
        public int SourceRecordId { get; set; }
        public int EmployeeId { get; set; }

        [ValidateNever]
        public Employee Employee { get; set; }
    }
}
