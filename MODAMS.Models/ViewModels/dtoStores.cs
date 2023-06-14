using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class dtoStores
    {
        public List<vwStore> vwStores = new List<vwStore>();
        public List<vwStoreEmployee> storeEmployees = new List<vwStoreEmployee>();

        //public decimal TotalCost()
        //{
        //    if (vwStores.Count == 0) return 0;
        //    decimal nTotalCost = 0;
        //    foreach (vwStore vwStore in vwStores) {
        //        nTotalCost += Convert.ToDecimal(vwStore.TotalCost);
        //    }
        //    return nTotalCost;
        //}
        //public int TotalAssets()
        //{
        //    if (vwStores.Count == 0) return 0;
        //    int nTotalAssets = 0; decimal nTotalCost = 0;

        //    foreach (vwStore vwStore in vwStores)
        //    {
        //        nTotalAssets += Convert.ToInt32(vwStore.TotalCount);
        //        nTotalCost += Convert.ToDecimal(vwStore.TotalCost);
        //    }
        //    return nTotalAssets;
        //}
        //public int StockBalance() {
        //    int nHandover = 0;
        //    int nDisposed = 0;
        //    return TotalAssets() - nHandover - nDisposed;
        //}
    }
}
