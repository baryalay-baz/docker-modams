using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class VerificationScheduleBarchartDTO
    {
        public int StoreId { get; set; }
        public int ScheduleId { get; set; }
        public string Result { get; set; } = string.Empty;
        public int VerificationRecordCount { get; set; }
    }
}
