using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class NotificationSection
    {
        [Key]
        public int Id { get; set; }
        public string SectionName { get; set; }
        public string area { get; set; }
        public string controller { get; set; }
        public string action { get; set; }
        
    }
}
