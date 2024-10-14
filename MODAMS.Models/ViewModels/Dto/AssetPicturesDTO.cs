using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class AssetPicturesDTO
    {
        private readonly List<AssetPicture> _assetPictures;
        private readonly int _picsPerPage;

        public AssetPicturesDTO(List<AssetPicture> assetPictures, int picsPerPage)
        {
            _assetPictures = assetPictures;
            _picsPerPage = picsPerPage;
            AssetPictures = assetPictures;

        }
        public List<AssetPicture> AssetPictures { get; set; } = new List<AssetPicture>();
        public int CurrentPage { get; set; }
        public int PageCount()
        {
            return Convert.ToInt32(Math.Ceiling(_assetPictures.Count / (double)_picsPerPage));
        }
        public List<AssetPicture> PaginatedPictures()
        {
            int start = (CurrentPage - 1) * _picsPerPage;
            return _assetPictures.OrderBy(m => m.Id).Skip(start).Take(_picsPerPage).ToList();
        }

        [ValidateNever]
        public int StoreId { get; set; }
        [ValidateNever]
        public string StoreName { get; set; } = string.Empty;
        [ValidateNever]
        public int AssetId { get; set; }

        [ValidateNever]
        public string AssetInfo { get; set; } = string.Empty;
    }
}
