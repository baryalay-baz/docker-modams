using Microsoft.AspNetCore.Mvc;
using MODAMS.Models.ViewModels.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices.IServices
{
    public interface IAlertService
    {
        Task<Result<AlertsDTO>> GetIndexAsync(int? departmentId = 0);
        Task<Result<int>> GetAlertCountAsync();
    }
}
