using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels.Dto
{
    public class StoreDTO
    {
        public vwStore vwStore { get; set; } = new vwStore();
        public List<Employee> employees { get; set; } = new List<Employee>();
        public List<vwStoreCategoryAsset> StoreCategoryAssets { get; set; } =
            new List<vwStoreCategoryAsset>();

        public List<Asset> storeAssets { get; set; } = new List<Asset>();

        public List<Asset> GetSubCategoryAssets(int id)
        {
            return storeAssets.Where(m => m.SubCategoryId == id).ToList();
        }
        public int RegisteredAssets { get; set; }
        public int TransferredAssets { get; set; }
        public int ReceivedAssets { get; set; }
        public int Handovers { get; set; }
        public int Disposals { get; set; }

        public int TotalAssets()
        {
            int total = 0;
            if (StoreCategoryAssets.Count > 0)
            {
                total = StoreCategoryAssets.Sum(m => m.TotalAssets);
            }
            return total;
        }
        public int GetBalance()
        {
            int balance;
            int transferred = TransferredAssets;
            int received = ReceivedAssets;
            int handover = Handovers;
            int disposed = Disposals;

            balance = RegisteredAssets - transferred + received - handover - disposed;

            return balance;
        }
        public string StoreOwnerInfo { get; set; }

        [ValidateNever]
        public int StoreId { get; set; }

        [ValidateNever]
        public string StoreName { get; set; } = string.Empty;

        [ValidateNever]
        public bool IsAuthorized { get; set; } = false;
    }
}
