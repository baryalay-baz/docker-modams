using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAMS.Models.ViewModels
{
    public class vwEmployees
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string DutyStation { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RoleName { get; set; }
    }
}
