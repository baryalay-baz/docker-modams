using MODAMS.Models;
using MODAMS.Models.ViewModels;

namespace MOD_AMS.Models
{
    public class StoreFinder
    {
        private int _nDeptId;
        private List<vwStores> _storeList;
        private List<vwStores> _storeResult = new List<vwStores>();
        private List<vwStores> _storeResultTemp = new List<vwStores>();

        public StoreFinder(int nDeptId, List<vwStores> storeList)
        {
            _nDeptId = nDeptId;
            _storeList = storeList;
        }
        public List<vwStores> GetStores()
        {
            var primaryStore = _storeList.Where(m => m.DepartmentId == _nDeptId).FirstOrDefault();
            if (primaryStore != null)
            {
                primaryStore.StoreType = 1;
                _storeResult.Add(primaryStore);
            }
            
            getSubStores(_nDeptId);
            _storeResult.AddRange(_storeResultTemp.OrderByDescending(m=>m.TotalCost).ToList());
            return _storeResult;
        }

        private void getSubStores(int? nDeptId)
        {
            List<vwStores> subStores = _storeList.Where(m => m.UpperLevelDeptId == nDeptId).ToList();

            int? nTempDeptId = 0;
            if (subStores.Count > 0)
            {
                foreach (vwStores store in subStores)
                {
                    nTempDeptId = store.DepartmentId;
                    store.StoreType = 2;
                    _storeResultTemp.Add(store);
                    getSubStores(nTempDeptId);
                }
            }
        }
    }
}