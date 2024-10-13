using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class NotificationRedirectorDTO
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Area { get; set; }
        public int? TargetRecordId { get; set; }
    }
}
