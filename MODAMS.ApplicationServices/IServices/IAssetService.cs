using Microsoft.AspNetCore.Http;
using MODAMS.Models.ViewModels.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.ApplicationServices.IServices
{
    public interface IAssetService
    {
        Task<Result<AssetsDTO>> GetIndexAsync(int storeId, int subCategoryId = 0);
        Task<Result<AssetListDTO>> GetAssetListAsync(int storeId);
        Task<Result<AssetCreateDTO>> GetCreateAssetAsync(int storeId);
        Task<Result<AssetCreateDTO>> CreateAssetAsync(AssetCreateDTO dto);
        Task<Result<AssetEditDTO>> GetEditAssetAsync(int assetId);
        Task<Result<AssetEditDTO>> EditAssetAsync(AssetEditDTO dto);
        Task<Result<AssetDocumentDTO>> GetAssetDocumentsAsync(int assetId);
        Task<Result> UploadDocumentAsync(int assetId, int documentTypeId, IFormFile? file);
        Task<Result<AssetInfoDTO>> GetAssetInfoAsync(int id, int page = 1, int tab = 1, int categoryId = 0);
        Task<Result<DeleteDocumentResultDTO>> DeleteDocumentAsync(int documentId);
        Task<Result<AssetPicturesDTO>> GetAssetPicturesAsync(int assetId);
        Task<Result<string>> UploadPictureAsync(int assetId, IFormFile? file);
        Task<Result<string>> DeletePictureAsync(int id, int assetId);
        Task<Result<string>> DeleteAssetAsync(int assetId);
        Task<Result<string>> RecoverAssetAsync(int assetId);



        Task<T> PopulateDtoAssetAsync<T>(T dto) where T : class, new();


        //API Calls
        Task<Result<string>> GetCategoriesAsync();
        Task<Result<string>> GetSubCategoriesAsync(int? categoryId);
        Task<Result<string>> GetDocumentTypesAsync();
        Task<Result<int>> GetLastAssetIdAsync();
    }
}
