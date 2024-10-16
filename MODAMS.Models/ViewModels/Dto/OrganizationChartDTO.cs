using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class OrganizationChartDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int? ParentID { get; set; }
        public bool Expanded { get; set; } = true;
        public string Avatar { get; set; }
    }
}
