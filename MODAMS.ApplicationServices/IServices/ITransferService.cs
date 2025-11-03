using MODAMS.Models.ViewModels.Dto;

namespace MODAMS.ApplicationServices.IServices
{
    public interface ITransferService
    {
        Task<Result<TransferDTO>> GetIndexAsync(int id = 0, int transferStatusId = 0, CancellationToken ct = default);
        Task<Result<TransferCreateDTO>> GetCreateTransferAsync();
        Task<Result<TransferCreateDTO>> CreateTransferAsync(TransferCreateDTO transferDTO, string selectedAssets);
        Task<Result<TransferEditDTO>> GetEditTransferAsync(int transferId);
        Task<Result<TransferEditDTO>> EditTransferAsync(TransferEditDTO transferDTO, string selectedAssets);
        Task<Result<TransferPreviewDTO>> GetPreviewTransferAsync(int transferId);
        Task<Result<string>> DeleteTransferAsync(int transferId);
        Task<Result> SubmitForAcknowledgementAsync(int transferId);
        Task<Result> AcknowledgeTransferAsync(int transferId);
        Task<Result> RejectTransferAsync(int transferId, string sReason = "");
        Task<Result<List<TransfersOutgoingAssetDTO>>> GetTransferredAssetsAsync(int storeId);
        Task<Result<List<TransfersIncomingAssetDTO>>> GetReceivedAssetsAsync(int storeId);
        Task<List<TransferChartDTO>> GetChartDataAsync(int type);
    }
}
