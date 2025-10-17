namespace MODAMS.Models.ViewModels.Dto
{
    public class AssetInfoDTO
    {
        public Asset Asset { get; set; }
        public List<AssetDocument> Documents { get; set; }
        public AssetPicturesDTO dtoAssetPictures { get; set; }
        public List<AssetHistory> AssetHistory { get; set; }
        public List<Transfer> Transfers { get; set; }

        public bool IsAuthorized { get; set; } = false;
    }
}
