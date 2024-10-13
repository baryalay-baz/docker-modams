using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace MODAMS.Models.ViewModels.Dto
{
    public class AlertsDTO
    {
        public int DepartmentId { get; set; }
        public List<vwAlert> Alerts { get; set; } = new List<vwAlert>();

        [ValidateNever]
        public IEnumerable<SelectListItem> DepartmentList { get; set; } = Enumerable.Empty<SelectListItem>();

        public List<AlertsChartData> GetChartData() {
            var assetTypeCounts = Alerts.GroupBy(alert => alert.AlertType)
                .Select(group => new AlertsChartData
                    {
                        Type = group.Key,
                        Count = group.Count()
                    }).ToList();

            return assetTypeCounts;
        }

        public List<AlertsBarChartData> GetBarChartData()
        {
            var alertTypeCounts = Alerts.GroupBy(alert => new { alert.AlertType, alert.Department })
                .Select(group => new AlertsBarChartData
                {
                    Department = group.Key.Department,
                    AlertType = group.Key.AlertType,
                    Count = group.Count()
                }).ToList();

            return alertTypeCounts;
        }

    }
    public class AlertsBarChartData { 
        public string Department { get; set; }
        public string AlertType { get; set; }
        public int Count { get; set; }
    }

    public class AlertsChartData { 
        public string Type { get; set; }
        public int Count { get; set; }
    }
}
