using MODAMS.Models;
using MODAMS.Models.ViewModels;

namespace MODAMS.Models
{
    public class StoreFinder
    {
        private readonly int _deptId;

        private List<vwStore> _storeList;
        private List<vwStore> _storeResultList = new List<vwStore>();
        private List<vwStore> _storeResultTempList = new List<vwStore>();

        public StoreFinder(int deptId, List<vwStore> storeList)
        {
            _deptId = deptId;
            _storeList = storeList;
        }
        public List<vwStore> GetStores()
        {
            var primaryStore = _storeList.Where(m => m.DepartmentId == _deptId).FirstOrDefault();
            if (primaryStore != null)
            {
                primaryStore.StoreType = 1;
                _storeResultList.Add(primaryStore);
            }
            
            getSubStores(_deptId);
            _storeResultList.AddRange(_storeResultTempList.OrderByDescending(m=>m.TotalCost).ToList());
            return _storeResultList;
        }

        private void getSubStores(int? deptId)
        {
            List<vwStore> subStores = _storeList.Where(m => m.UpperLevelDeptId == deptId).ToList();

            int? tempDeptId = 0;
            if (subStores.Count > 0)
            {
                foreach (vwStore store in subStores)
                {
                    tempDeptId = store.DepartmentId;
                    store.StoreType = 2;
                    _storeResultTempList.Add(store);
                    //recursion
                    getSubStores(tempDeptId);
                }
            }
        }
    }
}