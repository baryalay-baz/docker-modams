using Microsoft.AspNetCore.Http;
using MODAMS.Models.ViewModels.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices
{
    public interface IVerificationService
    {
        Task<Result<VerificationsDTO>> GetIndexAsync();
        Task<Result<VerificationScheduleCreateDTO>> GetCreateScheduleAsync();
        Task<Result<VerificationScheduleCreateDTO>> CreateScheduleAsync(VerificationScheduleCreateDTO dto, string teamMembersData);
        Task<Result<VerificationScheduleEditDTO>> GetEditScheduleAsync(int id);
        Task<Result<VerificationScheduleEditDTO>> EditScheduleAsync(VerificationScheduleEditDTO dto, string teamMembersData);
        Task<Result<VerificationSchedulePreviewDTO>> GetPreviewScheduleAsync(int id);
        Task<Result> DeleteScheduleAsync(int id);
        Task<Result<bool>> VerifyAssetAsync(VerificationSchedulePreviewDTO dto, IFormFile file);
        Task<Result> DeleteVerificationRecordAsync(int id);
        Task<Result<bool>> CompleteVerificationSchedule(int scheduleId);
    }
}
