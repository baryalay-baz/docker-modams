using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Utility;
using System.Data;


namespace MODAMSWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class DataTransferController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;

        public DataTransferController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TransferCategories() {

            DataTable tblCategories = DbFunc.GetTable("Select * from AssetCategory");
            DataTable tblSubCategories = new DataTable();

            try 
            {
                if (tblCategories.Rows.Count > 0)
                {
                    foreach (DataRow oRow in tblCategories.Rows)
                    {
                        var category = new Category()
                        {
                            CategoryCode = oRow["CategoryCode"].ToString(),
                            CategoryName = oRow["Category"].ToString()
                        };
                        await _db.Categories.AddAsync(category);
                        await _db.SaveChangesAsync();

                        tblSubCategories = DbFunc.GetTable("Select * from AssetSubCategory Where CategoryId=" + oRow["Id"]);
                        if (tblSubCategories.Rows.Count > 0)
                        {
                            foreach (DataRow oRowSubCategory in tblSubCategories.Rows)
                            {
                                var subCategory = new SubCategory()
                                {
                                    SubCategoryCode = oRowSubCategory["SubCategoryCode"].ToString(),
                                    SubCategoryName = oRowSubCategory["SubCategory"].ToString(),
                                    LifeSpan = (int)oRowSubCategory["Lifespan_months"],
                                    CategoryId = category.Id
                                };
                                await _db.SubCategories.AddAsync(subCategory);
                                await _db.SaveChangesAsync();
                            }
                        }
                    }
                    TempData["success"] = "Categories Transferred successfuly!";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }
            return View("Index");
        }

        public async Task<IActionResult> TransferAssets() {
            
            DataTable tblAssets = DbFunc.GetTable("Select * from Assets");
            
            if (tblAssets.Rows.Count > 0) {
                foreach (DataRow oRow in  tblAssets.Rows)
                {
                    Asset asset = new Asset();

                    asset.Name = oRow["AssetName"].ToString();
                    asset.Make = oRow["Make"].ToString();
                    asset.Model = oRow["Model"].ToString();
                    asset.Year = oRow["Year"].ToString();
                    asset.ManufacturingCountry = oRow["ManufacturingCountry"].ToString();
                    asset.SerialNo = oRow["SerialNo"].ToString();
                    asset.Barcode = oRow["ItemCode"].ToString();
                    asset.Engine = oRow["EngineNumber"].ToString();
                    asset.Chasis = oRow["ChasisNumber"].ToString();
                    asset.Plate = oRow["VINNumber"].ToString();
                    asset.Specifications = oRow["Specification"].ToString();
                    asset.Cost = (decimal)oRow["Cost"];
                    if ((oRow["PurchaseDate"])!= DBNull.Value)
                        asset.PurchaseDate = (DateTime)oRow["PurchaseDate"];

                    asset.PONumber = oRow["PONumber"].ToString();

                    if (oRow["RecieptDate"] != DBNull.Value)
                        asset.RecieptDate = (DateTime)oRow["RecieptDate"];
                    
                    asset.ProcuredBy = oRow["ProcuredBy"].ToString();
                    asset.Remarks = oRow["Remarks"].ToString();
                    asset.SubCategoryId = await GetAssetSubCategoryId(oRow["AssetSubCategoryId"].ToString());
                    asset.ConditionId = await GetConditionId(oRow["AssetConditionId"].ToString());
                    asset.StoreId = await GetStoreIdByAssetId(oRow["StoreId"].ToString());
                    asset.DonorId = await GetDonorId(oRow["DonorId"].ToString());
                        asset.AssetStatusId = await GetAssetStatusId(oRow["StatusId"].ToString());

                    await _db.Assets.AddAsync(asset);
                    await _db.SaveChangesAsync();

                    //try
                    //{

                    //}
                    //catch (Exception ex)
                    //{
                    //    TempData["error"] = ex.Message;
                    //    return RedirectToAction("Index", "DataTransfer");
                    //}
                }
                TempData["success"] = "Assets Transferred Successfuly!";
                return RedirectToAction("Index", "DataTransfer");
            }
            TempData["error"] = "unknown error occured!";
            return View();
        }

        private async Task<int> GetAssetSubCategoryId(string id) {
            DataRow oRow = DbFunc.GetFirstRow("AssetSubCategory","Id=" +  id);
            string sSubCategory = oRow["SubCategory"].ToString();

            int Id = await _db.SubCategories.Where(m=>m.SubCategoryName == sSubCategory)
                .Select(m=>m.Id).FirstOrDefaultAsync();

            return Id;
        }
        private async Task<int> GetConditionId(string id)
        {
            DataRow oRow = DbFunc.GetFirstRow("AssetConditions", "Id=" + id);
            string sCondition = oRow["Condition"].ToString();

            int Id = await _db.Conditions.Where(m => m.ConditionName == sCondition)
                .Select(m => m.Id).FirstOrDefaultAsync();

            return Id;
        }
        private async Task<int> GetStoreIdByAssetId(string id)
        {
            DataRow oRow = DbFunc.GetFirstRow("Stores", "Id=" + id);
            string sStore = oRow["Name"].ToString();
            
            int Id = await _db.Stores.Where(m => m.Name == sStore)
                .Select(m => m.Id).FirstOrDefaultAsync();

            return Id;
        }
        private async Task<int> GetDonorId(string id) {
            DataRow oRow = DbFunc.GetFirstRow("Donors", "Id=" + id);
            string sDonor = oRow["DonorName"].ToString();

            int Id = await _db.Donors.Where(m => m.Name == sDonor)
                .Select(m => m.Id).FirstOrDefaultAsync();

            return Id;
        }
        private async Task<int> GetAssetStatusId(string id) {
            DataRow oRow = DbFunc.GetFirstRow("AssetStatuses", "Id=" + id);
            string sStatus = oRow["Status"].ToString();

            int Id = await _db.AssetStatuses.Where(m => m.StatusName == sStatus)
                .Select(m => m.Id).FirstOrDefaultAsync();

            return Id;
        }
    }
}
