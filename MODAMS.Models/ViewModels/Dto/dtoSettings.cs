using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class dtoSettings
    {
        public List<LoginHistory> loginHistory = new List<LoginHistory>();
        public List<AuditLog> auditLog = new List<AuditLog>();

        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
        public List<SelectListItem> Months { get; set; }
        public List<SelectListItem> Years { get; set; }

        
    }
}
