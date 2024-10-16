using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class LogDTO
    {
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string OldVal { get; set; }
        public string NewVal { get; set; }
        public int ChangeType { get; set; }
    }
}
