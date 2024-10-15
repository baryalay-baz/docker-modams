using MODAMS.Models.ViewModels.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices
{
    public interface ITransferService
    {
        Task<Result<TransferDTO>> GetIndexAsync(int id = 0, int transferStatusId = 0);
        Task<Result<TransferCreateDTO>> GetCreateTransferAsync();
        Task<Result<TransferCreateDTO>> CreateTransferAsync(TransferCreateDTO transferDTO, string selectedAssets);
        Task<Result<TransferEditDTO>> GetEditTransferAsync(int transferId);
        Task<Result<TransferEditDTO>> EditTransferAsync(TransferEditDTO transferDTO, string selectedAssets);
        Task<Result<TransferPreviewDTO>> GetPreviewTransferAsync(int transferId);
        Task<Result> DeleteTransferAsync(int transferId);
        Task<Result> SubmitForAcknowledgementAsync(int transferId);
        Task<Result> AcknowledgeTransferAsync(int transferId);
        Task<Result> RejectTransferAsync(int transferId, string sReason = "");
        Task<Result<List<TransfersOutgoingAssetDTO>>> GetTransferredAssetsAsync(int storeId);
        Task<Result<List<TransfersIncomingAssetDTO>>> GetReceivedAssetsAsync(int storeId);



        Task<List<TransferChartDTO>> GetChartDataAsync(int type);
    }
}
