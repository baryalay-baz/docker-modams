using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MODAMS.Models;
using MODAMS.Models.ViewModels;


//using

namespace MODAMS.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationSection> NotificationSections { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DepartmentHead> DepartmentHeads { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Donor> Donors { get; set; }
        public DbSet<Category>  Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }   



        //Section for Views
        public DbSet<vwEmployees> vwEmployees { get; set; }
        public DbSet<vwDepartments> vwDepartments { get; set; }
        public DbSet<vwAvalableEmloyees> vwAvailableEmployees { get; set; }

    }
}

