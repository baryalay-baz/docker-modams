using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class SettingsDTO
    {
        public List<LoginHistory> loginHistory = new List<LoginHistory>();
        public List<AuditLog> auditLog = new List<AuditLog>();
        public List<Asset> deletedAssets= new List<Asset>();

        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
        public List<SelectListItem> Months { get; set; }
        public List<SelectListItem> Years { get; set; }

        
    }
}
