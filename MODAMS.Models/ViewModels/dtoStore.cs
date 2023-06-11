using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Models.ViewModels
{
    public class dtoStore
    {
        public vwStore vwStore { get; set; } = new vwStore();
        public List<Employee> employees { get; set; } = new List<Employee>();
        public List<vwStoreCategoryAsset> StoreCategoryAssets { get; set; } = 
            new List<vwStoreCategoryAsset>();

        public List<Asset> storeAssets { get; set; } = new List<Asset>();


        public List<Asset> GetSubCategoryAssets(int id) {
            return storeAssets.Where(m => m.SubCategoryId == id).ToList();
        }

        public int TotalAssets() {
            int total = 0;
            if (StoreCategoryAssets.Count > 0) { 
                total = StoreCategoryAssets.Sum(m=>m.TotalAssets);
            }
            return total;
        }
        public int GetBalance() {
            int balance;
            int transferred = 0;
            int handover = 0;
            int disposed = 0;

            balance = TotalAssets() - transferred - handover - disposed;

            return balance;
        }
        public string StoreOwner() {
            string sResult = "Vacant";
            string storeOwner = "";
            string emailAddress = "";
            string imageUrl = "";
            bool blnCheck = false;

            int employeeId = 0;
            if(vwStore != null)
            {
                employeeId = Convert.ToInt32(vwStore.EmployeeId);
            }

            if(employees.Count > 0)
            {
                if (employees.Count > 0) {
                    var rec = employees.Where(m => m.Id == employeeId).FirstOrDefault();
                    if (rec != null) {
                        storeOwner = rec.FullName;
                        emailAddress = rec.Email;
                        imageUrl = rec.ImageUrl;
                        blnCheck = true;
                    }
                }
            }
            if (blnCheck)
            {
                sResult = "<div class=\"d-flex align-items-center\">" +
                    "<div class=\"me-2\"><span>" +
                    "<img style=\"min-width:30px;\" src=\"" + imageUrl + "\" alt=\"profile-user\" class=\"data-image avatar avatar-lg rounded-circle\">" +
                    "</span></div><div><h6 class=\"mb-0\">" + storeOwner + "</h6><span class=\"text-muted fs-12\">" + emailAddress + "</span>\r\n" +
                    "</div></div>";
            }
            else {
                sResult = "<span class=\"text-secondary\">Store is vacant</span>";
            }

            return sResult;
        }
    }
}
