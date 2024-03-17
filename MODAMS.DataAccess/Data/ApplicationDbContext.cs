using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using System.Security.Claims;

namespace MODAMS.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
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
        public DbSet<TransferStatus> TransferStatuses { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<TransferDetail> TransferDetails { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<AssetHistory> AssetHistory { get; set; }
        public DbSet<DisposalType> DisposalTypes { get; set; }
        public DbSet<Disposal> Disposals { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }
        public DbSet<NewsFeed> NewsFeed { get; set; }
        public DbSet<LoginHistory> LoginHistory { get; set; }


        //Section for Views
        public DbSet<vwEmployees> vwEmployees { get; set; }
        public DbSet<vwDepartments> vwDepartments { get; set; }
        public DbSet<vwAvailableEmloyees> vwAvailableEmployees { get; set; }
        public DbSet<vwStore> vwStores { get; set; }
        public DbSet<vwStoreCategoryAsset> vwStoreCategoryAssets { get; set; }
        public DbSet<vwCategoryAsset> vwCategoryAssets { get; set; }
        public DbSet<vwTransfer> vwTransfers { get; set; }
        public DbSet<vwAsset> vwAssets { get; set; }
        public DbSet<vwTransferVoucher> vwTransferVouchers { get; set; }
        public DbSet<vwDisposal> vwDisposals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        //Temporarily stopped Logging Audit for Account Issues... 
        //Modifying SaveChanges for AuditLogs
        public override int SaveChanges()
        {
            var auditLogs = new List<AuditLog>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity.GetType() == typeof(IdentityUserRole<string>) ||
                    entry.Entity.GetType() == typeof(IdentityUser<string>) ||
                    entry.Entity.GetType() == typeof(IdentityRole<string>) ||
                    entry.Entity.GetType() == typeof(IdentityUserToken<string>)
                    )
                {
                    continue;
                }

                if (entry.State == EntityState.Modified || entry.State == EntityState.Added || entry.State == EntityState.Deleted)
                {
                    var action = "Modify";

                    if (entry.State == EntityState.Added)
                    {
                        action = "Add";
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        action = "Delete";
                    }

                    foreach (var property in entry.OriginalValues.Properties)
                    {
                        var original = entry.OriginalValues[property];
                        var current = entry.CurrentValues[property];

                        if (!object.Equals(original, current))
                        {
                            auditLogs.Add(new AuditLog
                            {
                                Timestamp = DateTime.Now,
                                EmployeeId = GetCurrentUserId(),
                                Action = action,
                                EntityName = entry.Entity.GetType().Name,
                                PrimaryKeyValue = entry.OriginalValues["Id"].ToString(), // Adjust as per your primary key.
                                PropertyName = property.Name,
                                OldValue = original?.ToString(),
                                NewValue = current?.ToString()
                            });
                        }
                    }
                }
            }
            AuditLog.AddRange(auditLogs);

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var auditLogs = new List<AuditLog>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity.GetType() == typeof(IdentityUserRole<string>) ||
                    entry.Entity.GetType() == typeof(IdentityUser<string>) ||
                    entry.Entity.GetType() == typeof(IdentityRole<string>) ||
                    entry.Entity.GetType() == typeof(IdentityUserToken<string>)
                    )
                {
                    continue;
                }
                var action = "Modify";

                if (entry.State == EntityState.Modified || entry.State == EntityState.Added || entry.State == EntityState.Deleted)
                {
                    if (entry.State == EntityState.Added)
                    {
                        action = "Add";
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        action = "Delete";
                    }
                }
                foreach (var property in entry.OriginalValues.Properties)
                {
                    var original = entry.OriginalValues[property];
                    var current = entry.CurrentValues[property];

                    if (action == "Modify")
                    {
                        if (!object.Equals(original, current))
                        {
                            auditLogs.Add(new AuditLog
                            {
                                Timestamp = DateTime.Now,
                                EmployeeId = GetCurrentUserId(),
                                Action = action,
                                EntityName = entry.Entity.GetType().Name,
                                PrimaryKeyValue = entry.OriginalValues["Id"].ToString(),
                                PropertyName = property.Name,
                                OldValue = original?.ToString(),
                                NewValue = current?.ToString()
                            });
                        }
                    }
                    else
                    {
                        auditLogs.Add(new AuditLog
                        {
                            Timestamp = DateTime.Now,
                            EmployeeId = GetCurrentUserId(),
                            Action = action,
                            EntityName = entry.Entity.GetType().Name,
                            PrimaryKeyValue = entry.OriginalValues["Id"].ToString(),
                            PropertyName = property.Name,
                            OldValue = original?.ToString(),
                            NewValue = current?.ToString()
                        });
                    }
                }
            }

            AuditLog.AddRange(auditLogs);
            return await base.SaveChangesAsync(cancellationToken);
        }

        private int? GetCurrentUserId()
        {
            int? EmployeeId = null;
            // Get the current claims principal
            var user = _httpContextAccessor.HttpContext.User;

            // Find the user's ID claim
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
                EmployeeId = DbFunctions.EmployeeId(userIdClaim?.Value);

            return EmployeeId;
        }
        private string GetPrimaryKeyValue(EntityEntry entry)
        {
            if (entry.Entity.ToString() =="AspNetUserRoles")
            {
                return "UserId";
            }
            else
            {
                // Handle other entities with a standard primary key property
                return entry.OriginalValues["Id"].ToString(); // Adjust as per your primary key.
            }
        }
    }
}

