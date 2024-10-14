using MODAMS.Models.ViewModels.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices
{
    public interface IReportingService
    {
        Task<ReportingDTO> PopulateDTO(ReportingDTO dto);
    }
}
