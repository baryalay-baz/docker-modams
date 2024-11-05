using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MODAMS.ApplicationServices.IServices;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using Newtonsoft.Json;

namespace MODAMS.ApplicationServices
{
    public class VerificationService : IVerificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly ILogger<VerificationService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly int _employeeId;
        private readonly int _countryOfficeId;

        public VerificationService(ApplicationDbContext db, IAMSFunc func, 
            IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment,
            ILogger<VerificationService> logger)
        {
            _db = db;
            _func = func;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;

            _employeeId = _func.GetEmployeeId();
        }

        public async Task<Result<VerificationsDTO>> GetIndexAsync()
        {
            try
            {
                var dto = new VerificationsDTO();
                var schedules = await _db.VerificationSchedules
                    .Include(m => m.Store).ThenInclude(m => m.Department)
                    .Include(m => m.VerificationTeams).ThenInclude(m => m.Employee)
                    .ToListAsync();

                if (IsInRole(SD.Role_User))
                {
                    var _supervisorEmployeeId = await _func.GetSupervisorIdAsync(_employeeId);
                    schedules = schedules
                        .Where(m => m.Store.Department.EmployeeId == _supervisorEmployeeId)
                        .ToList();
                }
                else if (IsInRole(SD.Role_StoreOwner))
                {
                    schedules = schedules
                        .Where(m => m.Store.Department.EmployeeId == _employeeId)
                        .ToList();
                }
                dto.Schedules = schedules;
                return Result<VerificationsDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<VerificationsDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<VerificationScheduleCreateDTO>> GetCreateScheduleAsync()
        {
            try
            {
                var dto = new VerificationScheduleCreateDTO();

                var store = await _db.Stores
                .Where(m => m.Department.EmployeeId == _employeeId).FirstOrDefaultAsync();

                if (store != null)
                {
                    dto.StoreId = store.Id;
                    dto.Store = store;
                    var assets = await _db.Assets.Where(m => m.StoreId == store.Id).ToListAsync();

                    dto.NumberOfAssets = assets.Count;
                }
                else {
                    return Result<VerificationScheduleCreateDTO>.Failure("Store not available");
                }

                var employees = await _db.Employees.ToListAsync();

                if (IsInRole(SD.Role_User))
                {
                    int _supervisorEmployeeId = await _func.GetSupervisorIdAsync(_employeeId);
                    employees = employees.Where(m => m.SupervisorEmployeeId == _supervisorEmployeeId).ToList();
                }
                else if (IsInRole(SD.Role_StoreOwner))
                {
                    employees = employees.Where(m => (m.Id == _employeeId) || (m.SupervisorEmployeeId == _employeeId)).ToList();
                }

                dto.Employees = employees;
                dto.EmployeesList = employees
                    .Select(m => new SelectListItem
                    {
                        Text = $"{m.FullName} ({m.JobTitle})",
                        Value = m.Id.ToString()
                    });

                return Result<VerificationScheduleCreateDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<VerificationScheduleCreateDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<VerificationScheduleCreateDTO>> CreateScheduleAsync(VerificationScheduleCreateDTO dto, string teamMembersData)
        {
            try
            {
                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var storeOwnerId = await _func.GetStoreOwnerIdAsync(dto.NewSchedule.StoreId);
                        dto.NewSchedule.EmployeeId = storeOwnerId;
                        dto.NewSchedule.VerificationStatus = "Pending";

                        await _db.VerificationSchedules.AddAsync(dto.NewSchedule);
                        await _db.SaveChangesAsync();

                        var selectedTeamMembers = JsonConvert.DeserializeObject<List<VerificationTeam>>(teamMembersData);
                        if (selectedTeamMembers != null)
                        {
                            var newVerificationTeam = new List<VerificationTeam>();

                            foreach (var teamMember in selectedTeamMembers)
                            {
                                newVerificationTeam.Add(new VerificationTeam
                                {
                                    EmployeeId = teamMember.EmployeeId,
                                    Role = teamMember.Role,
                                    VerificationScheduleId = dto.NewSchedule.Id
                                });
                                await NotifyTeamMemberAsync(dto.NewSchedule.Id, teamMember.EmployeeId, teamMember.Role);
                            }

                            if (newVerificationTeam.Any())
                            {
                                await _db.VerificationTeams.AddRangeAsync(newVerificationTeam);
                            }

                            await _db.SaveChangesAsync();
                        }
                        await transaction.CommitAsync();
                        return Result<VerificationScheduleCreateDTO>.Success(dto);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            await _db.Database.RollbackTransactionAsync();
                        }
                        catch (Exception rollbackEx)
                        {
                            _func.LogException(_logger, rollbackEx);
                            return Result<VerificationScheduleCreateDTO>.Failure(rollbackEx.Message);
                        }
                        _func.LogException(_logger, ex);
                        return Result<VerificationScheduleCreateDTO>.Failure(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<VerificationScheduleCreateDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<VerificationScheduleEditDTO>> GetEditScheduleAsync(int id)
        {
            try
            {
                var employees = await _db.Employees.ToListAsync();

                var storeList = await _db.Stores
                    .Include(m => m.Department)
                    .Include(m => m.Assets)
                    .Where(m => m.Department.EmployeeId == _employeeId).ToListAsync();

                var dto = await _db.VerificationSchedules
                    .AsNoTracking()
                    .Where(m => m.Id == id)
                    .Select(schedule => new VerificationScheduleEditDTO
                    {
                        Id = schedule.Id,
                        StartDate = schedule.StartDate,
                        EndDate = schedule.EndDate,
                        EmployeeId = schedule.EmployeeId,
                        VerificationStatus = schedule.VerificationStatus,
                        VerificationType = schedule.VerificationType,
                        NumberOfAssetsToVerify = schedule.NumberOfAssetsToVerify,
                        Notes = schedule.Notes,
                        StoreId = schedule.StoreId
                    })
                    .FirstOrDefaultAsync();

                if (dto == null) return Result<VerificationScheduleEditDTO>.Failure("Schedule not found!");

                dto.EditTeam = await _db.VerificationTeams
                    .AsNoTracking()
                    .Where(m => m.VerificationScheduleId == id)
                    .Select(member => new VerificationTeamEditDTO
                    {
                        Id = member.Id,
                        EmployeeId = member.EmployeeId,
                        Role = member.Role,
                        VerificationScheduleId = member.VerificationScheduleId
                    })
                    .ToListAsync();

                dto.Employees = employees;
                dto.Stores = storeList;
                dto.EmployeesList = await GetEmployeesSelectListAsync();
                dto.StoreList = await GetStoresSelectListAsync();

                return Result<VerificationScheduleEditDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<VerificationScheduleEditDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<VerificationScheduleEditDTO>> EditScheduleAsync(VerificationScheduleEditDTO dto, string teamMembersData)
        {
            try
            {
                var scheduleInDb = await _db.VerificationSchedules.FirstOrDefaultAsync(m => m.Id == dto.Id);

                if (scheduleInDb == null)
                    return Result<VerificationScheduleEditDTO>.Failure("Schedule not found!");


                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var storeOwnerId = await _func.GetStoreOwnerIdAsync(scheduleInDb.StoreId);
                        dto.EmployeeId = storeOwnerId;
                        dto.VerificationStatus = "Pending";
                        
                        // Update the schedule with the new values from dto
                        _db.Entry(scheduleInDb).CurrentValues.SetValues(dto);

                        await _db.SaveChangesAsync();

                        // Handle team members
                        if (!string.IsNullOrWhiteSpace(teamMembersData))
                        {
                            var selectedTeamMembers = JsonConvert.DeserializeObject<List<VerificationTeamEditDTO>>(teamMembersData);

                            if (selectedTeamMembers != null)
                            {
                                // Remove existing team members
                                var currentTeam = await _db.VerificationTeams
                                                           .Where(m => m.VerificationScheduleId == dto.Id)
                                                           .ToListAsync();

                                _db.VerificationTeams.RemoveRange(currentTeam);
                                await _db.SaveChangesAsync();

                                // Add new team members
                                var newTeamMembers = selectedTeamMembers.Select(teamMember => new VerificationTeam
                                {
                                    EmployeeId = teamMember.EmployeeId,
                                    Role = teamMember.Role,
                                    VerificationScheduleId = dto.Id
                                }).ToList();

                                await _db.VerificationTeams.AddRangeAsync(newTeamMembers);
                                await _db.SaveChangesAsync();
                            }
                        }
                        await transaction.CommitAsync();
                        return Result<VerificationScheduleEditDTO>.Success(dto);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            await _db.Database.RollbackTransactionAsync();
                        }
                        catch (Exception rollbackEx)
                        {
                            _func.LogException(_logger, rollbackEx);
                            return Result<VerificationScheduleEditDTO>.Failure(rollbackEx.Message);
                        }
                        _func.LogException(_logger, ex);
                        return Result<VerificationScheduleEditDTO>.Failure(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<VerificationScheduleEditDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<VerificationSchedulePreviewDTO>> GetPreviewScheduleAsync(int id)
        {
            try
            {
                var employees = await _db.Employees.Where(m => m.Id==_employeeId || m.SupervisorEmployeeId == _employeeId).ToListAsync();
                var storeList = await _db.Stores
                    .Include(m=>m.Department)
                    .Where(m => m.Department.EmployeeId == _employeeId).ToListAsync();

                var schedule = await _db.VerificationSchedules
                    .AsNoTracking()
                    .Where(m => m.Id == id)
                    .Select(schedule => new
                    {
                        schedule.Id,
                        schedule.StartDate,
                        schedule.EndDate,
                        schedule.EmployeeId,
                        schedule.VerificationStatus,
                        schedule.VerificationType,
                        schedule.Notes,
                        schedule.StoreId,
                        schedule.NumberOfAssetsToVerify
                    })
                    .FirstOrDefaultAsync();

                if (schedule == null) return Result<VerificationSchedulePreviewDTO>.Failure("Schedule Not found!");

                int verifiedAssets = await _db.VerificationRecords.Where(m => m.VerificationScheduleId == id).CountAsync();

                var dto = new VerificationSchedulePreviewDTO
                {
                    Id = schedule.Id,
                    StartDate = schedule.StartDate,
                    EndDate = schedule.EndDate,
                    EmployeeId = schedule.EmployeeId,
                    VerificationStatus = schedule.VerificationStatus,
                    VerificationType = schedule.VerificationType,
                    Notes = schedule.Notes,
                    StoreId = schedule.StoreId,
                    StoreName = await _func.GetStoreNameByStoreIdAsync(schedule.StoreId),
                    Assets = await GetVerificationAssetListAsync(schedule.StoreId, schedule.Id),
                    NumberOfAssetsToVerify = schedule.NumberOfAssetsToVerify,
                    VerifiedAssets = verifiedAssets
                };

                if (dto == null) return Result<VerificationSchedulePreviewDTO>.Failure("Schedule Not found!");

                var verificationTeam = await _db.VerificationTeams
                    .AsNoTracking()
                    .Include(m => m.Employee)
                    .Where(m => m.VerificationScheduleId == id)
                    .ToListAsync();

                dto.VerificationTeam = verificationTeam;

                dto.IsAuthorized = verificationTeam.Any(m => m.EmployeeId == _employeeId);

                dto.Employees = employees;
                dto.VerificationRecord = new VerificationRecord();
                dto.BarchartData = await GetBarchartDataAsync(id);
                dto.ProgressChart = await GetProgressChartAsync(id);

                return Result<VerificationSchedulePreviewDTO>.Success(dto);

            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result<VerificationSchedulePreviewDTO>.Failure(ex.Message);
            }
        }
        public async Task<Result<bool>> VerifyAssetAsync(VerificationSchedulePreviewDTO dto, IFormFile file)
        {
            if (dto.VerificationRecord == null)
            {
                return Result<bool>.Failure("Verification record is missing");
            }

            if (file == null || file.Length == 0)
            {
                return Result<bool>.Failure("No file uploaded or file is empty");
            }

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var verificationRecord = new VerificationRecord()
                    {
                        Id = dto.VerificationRecord.Id,
                        AssetId = dto.VerificationRecord.AssetId,
                        VerificationDate = dto.VerificationRecord.VerificationDate,
                        VerificationScheduleId = dto.VerificationRecord.VerificationScheduleId,
                        Result = dto.VerificationRecord.Result,
                        Comments = dto.VerificationRecord.Comments,
                        VerifiedBy = await _func.GetEmployeeNameAsync()
                    };

                    await _db.VerificationRecords.AddAsync(verificationRecord);
                    await _db.SaveChangesAsync();

                    var uploadResult = await UploadPictureAsync(dto.VerificationRecord.AssetId, file, verificationRecord.Id);
                    if (uploadResult != "success")
                    {
                        throw new Exception(uploadResult);
                    }

                    var scheduleInDb = await _db.VerificationSchedules
                        .FirstOrDefaultAsync(m => m.Id == dto.VerificationRecord.VerificationScheduleId);

                    if (scheduleInDb != null)
                    {
                        if (scheduleInDb.VerificationStatus == "Pending")
                        {
                            scheduleInDb.VerificationStatus = "Ongoing";
                            await _db.SaveChangesAsync();
                        }
                        var result = await UpdateScheduleStatus(scheduleInDb);
                        if (result.IsFailure)
                        {
                            throw new Exception($"Failed updating schedule status: {result.ErrorMessage}");
                        }
                    }
                    await transaction.CommitAsync();
                    return Result<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _func.LogException(_logger, ex);
                    return Result<bool>.Failure(ex.Message);
                }
            }
        }
        public async Task<Result<bool>> CompleteVerificationSchedule(int scheduleId)
        {
            var verificationSchedule = await _db.VerificationSchedules
                .FirstOrDefaultAsync(m => m.Id == scheduleId);

            if (verificationSchedule != null)
            {
                verificationSchedule.VerificationStatus = "Completed";
                await _db.SaveChangesAsync();
                await NotifyStoreOwnerAsync(verificationSchedule.Id);
            }

            return Result<bool>.Success(true);
        }
        public async Task<Result> DeleteScheduleAsync(int id)
        {
            try
            {
                var scheduleInDb = await _db.VerificationSchedules.FindAsync(id);

                if (scheduleInDb == null)
                    return Result.Failure("Schedule not found!");


                if (scheduleInDb.VerificationStatus != "Pending")
                    return Result.Failure("Only pending schedules can be deleted! Contact Administrator for deleting this schedule!");

                try
                {
                    _db.VerificationSchedules.Remove(scheduleInDb);
                    await _db.SaveChangesAsync();

                    //Log NewsFeed
                    string message = $"{await _func.GetEmployeeNameAsync()} deleted the verification schedule with id ({id})";
                    await _func.LogNewsFeedAsync(message, "Users", "Verifications", "PreviewSchedule", id);

                    return Result.Success();
                }
                catch (Exception ex)
                {
                    _func.LogException(_logger, ex);
                    return Result.Failure($"An unexpected error occurred while deleting the schedule with Id {id}");
                }
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result.Failure(ex.Message);
            }
        }
        public async Task<Result> DeleteVerificationRecordAsync(int id)
        {
            var recordInDb = await _db.VerificationRecords.FindAsync(id);

            if (recordInDb == null)
                return Result.Failure("Verification record not found!");

            try
            {
                //Delete picture from cloud storage
                var result = await DeletePictureAsync(recordInDb.ImageUrl);

                if (result.IsFailure)
                {
                    return Result.Failure("Error deleting file from bucket");
                }

                _db.VerificationRecords.Remove(recordInDb);
                await _db.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result.Failure($"An unexpected error occurred while deleting the Verification Record with Id {id}");
            }
        }


        //Private functions
        private bool IsInRole(string role) => _httpContextAccessor.HttpContext.User.IsInRole(role);
        private async Task NotifyTeamMemberAsync(int scheduleId, int employeeId, string role)
        {

            var scheduleInDb = await _db.VerificationSchedules.FirstOrDefaultAsync(m => m.Id == scheduleId);
            if (scheduleInDb == null)
                return;


            var notification = new Notification()
            {
                DateTime = DateTime.Now,
                EmployeeFrom = _employeeId,
                EmployeeTo = employeeId,
                Message = $"You have been assigned as {role} in a Verification Schedule with id {scheduleInDb.Id} by {await _func.GetEmployeeNameAsync()}!",
                NotificationSectionId = SD.NS_Schedule,
                Subject = "Asset Verification Schedule",
                IsViewed = false,
                TargetRecordId = scheduleInDb.Id
            };
            await _func.SendNotificationAsync(notification);
        }
        private async Task NotifyStoreOwnerAsync(int scheduleId)
        {
            // Fetch the schedule with related store and department data
            var scheduleInDb = await _db.VerificationSchedules
                .Include(m => m.Store)
                .ThenInclude(m => m.Department)
                .FirstOrDefaultAsync(m => m.Id == scheduleId);

            if (scheduleInDb == null)
            {
                // Log or handle the case where the schedule is not found
                throw new InvalidOperationException($"Schedule with ID {scheduleId} not found.");
            }

            // Extract the employee ID if available
            int employeeId = scheduleInDb.Store?.Department?.EmployeeId ?? 0;

            // Create a notification object
            var notification = new Notification
            {
                DateTime = DateTime.Now,
                EmployeeFrom = _employeeId,  // Assumes _employeeId is set and available
                EmployeeTo = employeeId,
                Message = $"Verification Schedule with ID {scheduleInDb.Id} completed successfully!",
                NotificationSectionId = SD.NS_Schedule,
                Subject = "Verification Completed",
                IsViewed = false,
                TargetRecordId = scheduleInDb.Id
            };

            // Send the notification
            await _func.SendNotificationAsync(notification);

            // Log the completion in the news feed
            await _func.LogNewsFeedAsync(
                $"Verification Schedule of Project: {scheduleInDb.Store.Name} has been completed",
                "Users",
                "Verifications",
                "PreviewSchedule",
                scheduleInDb.Id
            );
        }
        private async Task<List<SelectListItem>> GetEmployeesSelectListAsync()
        {
            var employees = await _db.Employees
                .Where(m => m.Id == _employeeId|| m.SupervisorEmployeeId== _employeeId)
                .ToListAsync();

            return employees
                .Select(m => new SelectListItem
                {
                    Text = $"{m.FullName} ({m.JobTitle})",
                    Value = m.Id.ToString()
                }).ToList();
        }
        private async Task<List<SelectListItem>> GetStoresSelectListAsync()
        {
            var storeList = await _db.Stores
                .Include(m=>m.Department)
                .Where(m => m.Department.EmployeeId == _employeeId)
                .ToListAsync();

            return storeList
                .Select(m => new SelectListItem
                {
                    Text = $"{m.Name}",
                    Value = m.Id.ToString()
                }).ToList();
        }
        private async Task<List<VerificationAssetsDTO>> GetVerificationAssetListAsync(int storeId, int scheduleId)
        {
            var assets = await _db.Assets
                    .Where(m => m.StoreId == storeId && m.AssetStatusId != SD.Asset_Deleted).ToListAsync();

            if (!assets.Any())
                return new List<VerificationAssetsDTO>();

            // Get all the verification records for the current verification schedule
            var verificationRecords = await _db.VerificationRecords
                .Where(v => v.VerificationScheduleId == scheduleId)
                .ToListAsync();

            // Create a dictionary to make lookups faster by AssetId
            var verificationRecordsDict = verificationRecords
                .GroupBy(v => v.AssetId)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault());

            // Create the list of VerificationAssetsDTO
            var verificationAssetsDtoList = assets.Select(asset => new VerificationAssetsDTO
            {
                Id = asset.Id,
                Make = asset.Make,
                Model = asset.Model,
                Name = asset.Name,
                SerialNo = asset.SerialNo,
                Barcode = asset.Barcode,
                IsSelected = verificationRecordsDict.ContainsKey(asset.Id),
                VerifiedBy = verificationRecordsDict.ContainsKey(asset.Id) ? verificationRecordsDict[asset.Id].VerifiedBy : "Not Verified",
                Result = verificationRecordsDict.ContainsKey(asset.Id) ? verificationRecordsDict[asset.Id].Result : "Not Verified",
                VerificationRecordId = verificationRecordsDict.ContainsKey(asset.Id) ? verificationRecordsDict[asset.Id].Id : 0
            }).ToList();


            return verificationAssetsDtoList;
        }
        private async Task<List<VerificationScheduleBarchartDTO>> GetBarchartDataAsync(int scheduleId)
        {
            var result = await _db.VerificationSchedules
            .Join(_db.VerificationRecords,
                schedule => schedule.Id,
                record => record.VerificationScheduleId,
                (schedule, record) => new
                {
                    schedule.StoreId,
                    schedule.Id,
                    record.Result
                })
            .GroupBy(x => new { x.StoreId, x.Id, x.Result })
            .Select(g => new VerificationScheduleBarchartDTO
            {
                StoreId = g.Key.StoreId,
                ScheduleId = g.Key.Id,
                Result = g.Key.Result,
                VerificationRecordCount = g.Count()
            }).ToListAsync();

            result = result.Where(m => m.ScheduleId == scheduleId).ToList();

            return result;
        }
        private async Task<List<ProgressChartItemDTO>> GetProgressChartAsync(int scheduleId)
        {
            var schedule = await _db.VerificationSchedules
                .Include(m => m.Store).ThenInclude(m => m.Assets)
                .Where(m => m.Id == scheduleId).FirstOrDefaultAsync();

            if (schedule == null)
                return new List<ProgressChartItemDTO>();

            int assetCount = schedule.NumberOfAssetsToVerify;

            TimeSpan dateDifference = schedule.EndDate - schedule.StartDate;


            int numberOfDays = dateDifference.Days > 0 ? dateDifference.Days + 1 : 1;


            double assetsPerDay = (double)assetCount / numberOfDays;
            double cumulativePlannedAssets = 0;
            double? cumulativeActualAssets = 0;

            var result = new List<ProgressChartItemDTO>();

            var lastVerificationDate = await _db.VerificationRecords
                .Where(vr => vr.VerificationScheduleId == schedule.Id)
                .OrderByDescending(vr => vr.VerificationDate)
                .Select(vr => vr.VerificationDate)
                .FirstOrDefaultAsync();

            if (lastVerificationDate == DateTime.MinValue)
                lastVerificationDate = schedule.StartDate;

            for (int i = 1; i <= numberOfDays; i++)
            {
                cumulativePlannedAssets += assetsPerDay;

                DateTime currentDay = schedule.StartDate.AddDays(i - 1);

                if (currentDay <= lastVerificationDate)
                {
                    int verifiedAssetsToday = await GetVerifiedAssetsForDay(currentDay, schedule.Id);
                    cumulativeActualAssets += verifiedAssetsToday;
                }
                else
                {
                    cumulativeActualAssets = null;
                }

                var chartItem = new ProgressChartItemDTO()
                {
                    Day = i,
                    PlanProgress = Math.Round(cumulativePlannedAssets, 2),
                    Progress = cumulativeActualAssets,
                    ScheduleId = schedule.Id,
                    Date = currentDay
                };

                result.Add(chartItem);
            }

            return result;
        }
        private async Task<int> GetVerifiedAssetsForDay(DateTime date, int id)
        {
            var result = await _db.VerificationRecords
                      .Where(vr => vr.VerificationScheduleId == id && vr.VerificationDate.Date == date.Date)
                      .CountAsync();

            return result;
        }
        private async Task<Result> UpdateScheduleStatus(VerificationSchedule scheduleInDb)
        {

            var verificationRecordCount = await _db.VerificationRecords.CountAsync(m => m.VerificationScheduleId == scheduleInDb.Id);
            var assetCount = await _db.Assets.CountAsync(m => m.StoreId == scheduleInDb.StoreId);

            if (assetCount == verificationRecordCount)
            {
                try
                {
                    return await CompleteVerificationSchedule(scheduleInDb.Id);
                }
                catch (Exception ex)
                {
                    _func.LogException(_logger, ex);
                    return Result.Failure($"Error occurred while completing Schedule: {ex.Message}");
                }
            }

            return Result.Success();
        }
        private async Task<string> UploadPictureAsync(int assetId, IFormFile? file, int verificationRecordId)
        {
            if (file == null)
            {
                return "File not found!";
            }

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            Guid guid = Guid.NewGuid();
            string fileName = guid + Path.GetExtension(file.FileName);
            string path = Path.Combine(wwwRootPath, "assetpictures");

            try
            {
                // Save the file to disk
                using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Save the asset picture to the database
                var assetPicture = new AssetPicture
                {
                    ImageUrl = "/assetpictures/" + fileName,
                    AssetId = assetId
                };
                await _db.AssetPictures.AddAsync(assetPicture);
                await _db.SaveChangesAsync();

                var verificationRecord = await _db.VerificationRecords.Where(m => m.Id == verificationRecordId).FirstOrDefaultAsync();
                if (verificationRecord != null)
                    verificationRecord.ImageUrl = assetPicture.ImageUrl;

                await _db.SaveChangesAsync();

                return "success";
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return ex.Message;
            }
        }
        private async Task<Result> DeletePictureAsync(string imageUrl)
        {
            if (imageUrl == "")
            {
                return Result.Failure("Picture not found!");
            }

            var assetPicture = await _db.AssetPictures.Where(m => m.ImageUrl == imageUrl).FirstOrDefaultAsync();
            if (assetPicture == null)
            {
                return Result.Failure("Picture not found!");
            }
                        
            // Remove the picture entry from the database
            try
            {
                _db.AssetPictures.Remove(assetPicture);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _func.LogException(_logger, ex);
                return Result.Failure($"Error occurred while removing the picture from the database: {ex.Message}");
            }

            // Remove file from disk
            string sFileName = assetPicture.ImageUrl.Substring(15); // Removes "/assetpictures/"
            if (!await DeleteFileAsync(sFileName, "assetpictures"))
            {
                return Result.Failure("Error deleting picture!");
            }

            return Result.Success();
        }
        private async Task<bool> DeleteFileAsync(string fileName, string folderName)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string folderPath = Path.Combine(wwwRootPath, folderName);
            string filePath = Path.Combine(folderPath, fileName);

            try
            {
                await Task.Run(() => System.IO.File.Delete(filePath));
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
