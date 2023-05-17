using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int EmployeeId { get; set; }
        public Employee Employees { get; set; }
    }
}
