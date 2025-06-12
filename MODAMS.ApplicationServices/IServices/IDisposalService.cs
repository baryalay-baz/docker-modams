using MODAMS.Models.ViewModels.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices.IServices
{
    public interface IDisposalService
    {
        Task<Result<DisposalsDTO>> GetIndexAsync();
        Task<Result<DisposalCreateDTO>> GetCreateDisposalAsync();
        Task<Result<DisposalCreateDTO>> CreateDisposalAsync(DisposalCreateDTO dto);
        Task<Result<DisposalEditDTO>> GetEditDisposalAsync(int id);
        Task<Result<DisposalEditDTO>> EditDisposalAsync(DisposalEditDTO dto);
        Task<Result<DisposalPreviewDTO>> GetDisposalPreviewAsync(int id);
        Task<Result<bool>> DeleteDisposalAsync(int id);
        Task<T> PopulateDisposalDtoAsync<T>(T dto) where T : class;
    }
}
