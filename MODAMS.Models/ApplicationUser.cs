using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;


namespace MODAMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int EmployeeId { get; set; }

        [NotMapped]
        public Employee Employees { get; set; }
    }
}
