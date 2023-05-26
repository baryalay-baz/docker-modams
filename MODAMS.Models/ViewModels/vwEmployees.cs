using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class vwEmployees
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RoleName { get; set; }
        public int SupervisorEmployeeId { get; set; }
        public string SupervisorName { get; set; }
        public string ImageUrl { get; set; }
        public string CardNumber { get; set; }
        public string DisplayMode { get; set; }
        public bool IsActive { get; set; }

    }
}
