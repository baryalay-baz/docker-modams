using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MODAMS.ApplicationServices.IServices;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices
{
    public class StoreService : IStoreService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly ILogger<StoreService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private int _employeeId;
        private bool _isSomali;
        public StoreService(ApplicationDbContext db, IAMSFunc func, ILogger<StoreService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _func = func;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _employeeId = _func.GetEmployeeId();
            _isSomali = CultureInfo.CurrentUICulture.Name == "so";
        }
        public async Task<Result<StoresDTO>> GetIndexAsync()
        {
            try
            {
                var stores = await _func.GetStoresByEmployeeIdAsync(_employeeId);

                var storeEmployees = new List<vwStoreEmployee>();

                foreach (var store in stores)
                {
                    if (store != null)
                    {
                        if (store.EmployeeId > 0)
                        {
                            var empl = await _db.vwEmployees
                                .FirstOrDefaultAsync(m => m.Id == store.EmployeeId);

                            if (empl != null)
                            {
                                vwStoreEmployee se = new vwStoreEmployee()
                                {
                                    Id = empl.Id,
                                    FullName = empl.FullName,
                                    Email = empl.Email,
                                    Role = empl.RoleName,
                                    ImageUrl = empl.ImageUrl,
                                    StoreId = store.Id
                                };
                                storeEmployees.Add(se);

                                //Add store users
                                var storeUsers = await _db.vwEmployees
                                    .Where(m => m.SupervisorEmployeeId == empl.Id && m.RoleName == "User")
                                    .ToListAsync();

                                foreach (var user in storeUsers)
                                {
                                    vwStoreEmployee su = new vwStoreEmployee()
                                    {
                                        Id = user.Id,
                                        ImageUrl = user.ImageUrl,
                                        FullName = user.FullName,
                                        Email = user.Email,
                                        Role = user.RoleName,
                                        StoreId = store.Id
                                    };
                                    storeEmployees.Add(su);
                                }
                            }
                        }
                    }
                }

                var dto = new StoresDTO()
                {
                    vwStores = stores,
                    storeEmployees = storeEmployees
                };

                //update dtoReportingModel for the partial view
                dto.dtoReporting.AssetStores = stores.ToList().Select(m => new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString()
                });
                var categories = await _db.Categories.Select(m => new SelectListItem
                {
                    Text = _isSomali ? m.CategoryNameSo : m.CategoryName,
                    Value = m.Id.ToString()
                }).ToListAsync();
                var subCategories = await _db.SubCategories.Select(m => new SelectListItem
                {
                    Text = _isSomali ? m.SubCategoryNameSo : m.SubCategoryName,
                    Value = m.Id.ToString()
                }).ToListAsync();
                var assetStatuses = await _db.AssetStatuses.Select(m => new SelectListItem
                {
                    Text = _isSomali ? m.StatusNameSo : m.StatusName,
                    Value = m.Id.ToString()
                }).ToListAsync();
                var assetConditions = await _db.Conditions.Select(m => new SelectListItem
                {
                    Text = _isSomali ? m.ConditionNameSo : m.ConditionName,
                    Value = m.Id.ToString()
                }).ToListAsync();
                var donors = await _db.Donors.Select(m => new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString()
                }).ToListAsync();

                dto.dtoReporting.AssetStatuses = assetStatuses;
                dto.dtoReporting.Categories = categories;
                dto.dtoReporting.SubCategories = subCategories;
                dto.dtoReporting.Conditions = assetConditions;
                dto.dtoReporting.Donors = donors;

                return Result<StoresDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<StoresDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<StoreDTO>> GetStoreDetailsAsync(int storeId)
        {
            try
            {
                var vwStore = await _db.vwStores.FirstOrDefaultAsync(m => m.Id == storeId);

                if (vwStore == null)
                {
                    return Result<StoreDTO>.Failure("Store not available!");
                }


                var dto = new StoreDTO
                {
                    vwStore = vwStore,
                    employees = new List<Employee>(),
                    StoreOwnerInfo = await _func.GetStoreOwnerInfoAsync(storeId)
                };

                var empId = IsInRole("User") ? await _func.GetSupervisorIdAsync(_employeeId) : _employeeId;
                if (empId == await _func.GetStoreOwnerIdAsync(storeId))
                    dto.IsAuthorized = true;

                var storeOwnerId = vwStore.EmployeeId;
                if (storeOwnerId > 0)
                {
                    var storeOwner = await _db.Employees.FirstOrDefaultAsync(e => e.Id == storeOwnerId);
                    if (storeOwner != null)
                    {
                        dto.employees.Add(storeOwner);
                        dto.employees.AddRange(await _db.Employees
                            .Where(e => e.SupervisorEmployeeId == storeOwnerId)
                            .ToListAsync());
                    }
                }
                dto.storeAssets = await _db.Assets
                    .Where(a => a.AssetStatusId != SD.Asset_Deleted && a.StoreId == storeId)
                    .Include(a => a.SubCategory)
                    .Include(a => a.Condition)
                    .Include(a => a.AssetStatus)
                    .ToListAsync();
                dto.StoreCategoryAssets = await _db.vwStoreCategoryAssets
                    .Where(sca => sca.StoreId == storeId).OrderBy(m => m.CategoryId)
                    .ToListAsync();

                dto.TransferredAssets = await _db.TransferDetails
                .Include(td => td.Transfer)
                    .CountAsync(td => td.Transfer.StoreFromId == storeId && td.Transfer.TransferStatusId == SD.Transfer_Completed);

                dto.ReceivedAssets = await _db.TransferDetails
                .Include(td => td.Transfer)
                    .CountAsync(td => td.Transfer.StoreId == storeId && td.Transfer.TransferStatusId == SD.Transfer_Completed);

                dto.Handovers = 0;
                dto.Disposals = await _db.Assets.Where(m => m.StoreId == storeId && m.AssetStatusId == SD.Asset_Disposed).CountAsync();

                dto.StoreId = storeId;
                dto.StoreName = _isSomali ? vwStore.NameSo : vwStore.Name;
                return Result<StoreDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<StoreDTO>.Failure(ex.Message);
            }
        }

        //Private functions
        private bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
}
