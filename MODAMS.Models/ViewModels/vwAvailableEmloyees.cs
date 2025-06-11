using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class vwAvailableEmloyees
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
