using Microsoft.AspNetCore.Mvc.Rendering;
using MODAMS.Models.ViewModels;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using Microsoft.AspNetCore.Hosting;
using MODAMS.DataAccess.Data;
using MODAMS.Utility;
using Telerik.Reporting.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace MODAMS.ApplicationServices
{
    public class ReportingService : IReportingService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ReportingService> _logger;

        private int _employeeId;
        private int _supervisorEmployeeId;
        private int _storeId;
        public ReportingService(ApplicationDbContext db, IAMSFunc func,IHttpContextAccessor httpContextAccessor, ILogger<ReportingService> logger)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _func = func;
            _logger = logger;

            _employeeId = _func.GetEmployeeId();
            _supervisorEmployeeId = _func.GetSupervisorId(_employeeId);
        }

        private bool IsInRole(string role) => _httpContextAccessor.HttpContext.User.IsInRole(role);
        public async Task<ReportingDTO> PopulateDTO(ReportingDTO dto)
        {
            //Populate Asset Report
            var stores = await _db.vwStores.OrderByDescending(m => m.TotalCount).ToListAsync();
            var allStores = stores;

            if (IsInRole("User"))
            {
                stores = stores.Where(m => m.EmployeeId == _supervisorEmployeeId).ToList();
            }
            else if (IsInRole("StoreOwner"))
            {
                stores = stores.Where(m => m.EmployeeId == _employeeId).ToList();
                if (stores.Count > 0)
                {
                    vwStore store = stores.First();
                    if (store != null)
                    {
                        int nDeptId = (int)store.DepartmentId;
                        var storeFinder = new StoreFinder(nDeptId, allStores);
                        stores = storeFinder.GetStores();
                    }
                }
                else
                {
                    stores = new List<vwStore>();
                }
            }

            var storeSelectList = stores.ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            });
            var categories = await _db.Categories.Select(m => new SelectListItem
            {
                Text = m.CategoryName,
                Value = m.Id.ToString()
            }).ToListAsync();
            var subCategories = await _db.SubCategories.Select(m => new SelectListItem
            {
                Text = m.SubCategoryName,
                Value = m.Id.ToString()
            }).ToListAsync();
            var assetStatuses = await _db.AssetStatuses.Select(m => new SelectListItem
            {
                Text = m.StatusName,
                Value = m.Id.ToString()
            }).ToListAsync();
            var assetConditions = await _db.Conditions.Select(m => new SelectListItem
            {
                Text = m.ConditionName,
                Value = m.Id.ToString()
            }).ToListAsync();
            var donors = await _db.Donors.Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            }).ToListAsync();

            dto.AssetStores = storeSelectList;
            dto.AssetStatuses = assetStatuses;
            dto.Categories = categories;
            dto.SubCategories = subCategories;
            dto.Conditions = assetConditions;
            dto.Donors = donors;

            //Populate Transfer Report
            dto.TransferStores = allStores.ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            });
            dto.TransferStatuses = _db.TransferStatuses.ToList().Select(m => new SelectListItem
            {
                Text = m.Status,
                Value = m.Id.ToString()
            });

            //Populate Disposal= Report
            var disposalTypes = await _db.DisposalTypes.ToListAsync();
            dto.DisposalTypes = disposalTypes.ToList().Select(m => new SelectListItem
            {
                Text = m.Type,
                Value = m.Id.ToString()
            });

            return dto;
        }
    }
}
