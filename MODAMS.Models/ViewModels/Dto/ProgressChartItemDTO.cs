using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class ProgressChartItemDTO
    {
        public int Day { get; set; }
        public double PlanProgress { get; set; }
        public double? Progress { get; set; }
        public int ScheduleId { get; set; }
        public DateTime Date { get; set; }
        public string FormattedDate => Date.ToString("yyyy-MM-dd");
    }
}
