using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class dtoAssetPictures
    {
        private readonly List<AssetPicture> _assetPictures;
        private readonly int _picsPerPage;
        
        public dtoAssetPictures(List<AssetPicture> assetPictures, int picsPerPage)
        {
            _assetPictures = assetPictures;
            _picsPerPage = picsPerPage;
            
        }

        public int CurrentPage { get; set; }
        public int PageCount()
        {
            return Convert.ToInt32(Math.Ceiling(_assetPictures.Count / (double)_picsPerPage));
        }
        public List<AssetPicture> PaginatedPictures() {
            int start = (CurrentPage - 1) * _picsPerPage;
            return _assetPictures.OrderBy(m=>m.Id).Skip(start).Take(_picsPerPage).ToList();
        }
    }
}
