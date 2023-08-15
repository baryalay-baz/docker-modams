using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using System.Reflection.Emit;


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
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<AssetStatus> AssetStatuses { get; set; }
        public DbSet<Condition> Conditions { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<AssetDocument> AssetDocuments { get; set; }
        public DbSet<AssetDocumentChecklist> AssetDocumentChecklist { get; set; }
        public DbSet<AssetPicture> AssetPictures { get; set; }
        public DbSet<Log> Log { get; set; }
        public DbSet<TransferStatus> TransferStatuses { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<TransferDetail> TransferDetails { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<AssetHistory> AssetHistory { get; set; }
        public DbSet<DisposalType> DisposalTypes { get; set; }
        public DbSet<Disposal> Disposals { get; set; }


        //Section for Views
        public DbSet<vwEmployees> vwEmployees { get; set; }
        public DbSet<vwDepartments> vwDepartments { get; set; }
        public DbSet<vwAvalableEmloyees> vwAvailableEmployees { get; set; }
        public DbSet<vwStore> vwStores { get; set; }
        public DbSet<vwStoreCategoryAsset> vwStoreCategoryAssets { get; set; }
        public DbSet<vwCategoryAsset> vwCategoryAssets { get; set; }
        public DbSet<vwTransfer> vwTransfers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}

