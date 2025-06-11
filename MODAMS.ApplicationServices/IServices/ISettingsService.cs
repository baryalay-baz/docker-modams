using MODAMS.Models.ViewModels.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices.IServices
{
    public interface ISettingsService
    {
        Task<Result<SettingsDTO>> GetIndexAsync(int? nMonth, int? nYear);
        Task<Result<int>> SetStoreEmployeesAsync();
    }
}
