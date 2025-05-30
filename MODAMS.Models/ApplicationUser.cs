using Microsoft.AspNetCore.Identity;


namespace MODAMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int EmployeeId { get; set; }
        public Employee Employees { get; set; } = new Employee();
    }
}
