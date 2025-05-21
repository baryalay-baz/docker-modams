
using System.Globalization;

namespace MODAMS.Models.ViewModels.Dto
{
    public class VerificationsDTO
    {
        public List<VerificationSchedule> Schedules { get; set; }

        public List<ScheduleBarChartItem> GetBarChartData()
        {
            if (Schedules != null && Schedules.Any())
            {
                var barChartData = Schedules
                    .GroupBy(s => s.VerificationStatus)
                    .Select(group => new ScheduleBarChartItem
                    {
                        ScheduleStatus = group.Key,
                        ScheduleCount = group.Count(),
                        StoreId = group.First().StoreId
                    })
                    .ToList();

                return barChartData;
            }

            return new List<ScheduleBarChartItem>();
        }
    }

    public class ScheduleBarChartItem
    {
        public string ScheduleStatus { get; set; } = string.Empty;
        public int ScheduleCount { get; set; }
        public int StoreId { get; set; }
    }
}
