using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    }
}
