using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Telerik.Reporting;
using Telerik.Reporting.Services;

namespace MODAMS.Utility
{
    public class CustomReportSourceResolver : IReportSourceResolver
    {
        private string ReportsPath = "Reports";
        private ApplicationDbContext _db;
        public CustomReportSourceResolver(ApplicationDbContext db)
        {
            _db = db;
        }

        public ReportSource Resolve(string reportId, OperationOrigin operationOrigin, IDictionary<string, object> currentParameterValues)
        {
            string reportPath = Path.Combine(this.ReportsPath, reportId);
            var reportPackager = new ReportPackager();
            Report report = null;
            using (var sourceStream = System.IO.File.OpenRead(reportPath))
            {
                report = (Report)reportPackager.UnpackageDocument(sourceStream);
            }
            if (operationOrigin == OperationOrigin.GenerateReportDocument)
            {
                // Set the data source for the report
                var vwAssets = GetAssetList().GetAwaiter().GetResult();
                
                if (vwAssets != null)
                {
                    report.DataSource = vwAssets;
                    // Set the data source for another data item
                    var graph1 = report.Items.Find("graph1", true)[0] as Graph;
                    graph1.DataSource = vwAssets;
                }
            };
            return new InstanceReportSource
            {
                ReportDocument = report
            };
        }

        public async Task<List<vwAsset>> GetAssetList()
        {
            var vwAssets = await _db.Assets.Where(m => m.AssetStatusId != SD.Asset_Deleted)
                .Include(m => m.SubCategory).ThenInclude(m => m.Category)
                .Include(m => m.Condition)
                .Include(m => m.Store).ThenInclude(m => m.Department)
                .Include(m => m.AssetStatus)
                .Include(m => m.Donor)
                .Select(asset => new vwAsset
                {
                    Id = asset.Id,
                    CategoryId = asset.SubCategory.CategoryId,
                    CategoryName = asset.SubCategory.Category.CategoryName,
                    SubCategoryId = asset.SubCategoryId,
                    SubCategoryName = asset.SubCategory.SubCategoryName,
                    Name = asset.Name,
                    Make = asset.Make,
                    Model = asset.Model,
                    Year = asset.Year,
                    ManufacturingCountry = asset.ManufacturingCountry,
                    SerialNo = asset.SerialNo,
                    BarCode = asset.Barcode,
                    Engine = asset.Engine,
                    Chasis = asset.Chasis,
                    Plate = asset.Plate,
                    Specifications = asset.Specifications,
                    Cost = asset.Cost,
                    PurchaseDate = asset.PurchaseDate,
                    PONumber = asset.PONumber,
                    RecieptDate = asset.RecieptDate,
                    ProcuredBy = asset.ProcuredBy,
                    Remarks = asset.Remarks,
                    ConditionId = asset.ConditionId,
                    StoreId = asset.StoreId,
                    AssetStatusId = asset.AssetStatusId,
                    DonorId = asset.DonorId,
                    DonorName = asset.Donor.Name,
                    DepartmentId = asset.Store.DepartmentId,
                    DepartmentName = asset.Store.Department.Name,
                    EmployeeId = asset.Store.Department.EmployeeId,
                    StoreOwner = "-",
                    Identification = asset.SubCategory.Category.CategoryName == "Vehicles" ? "Plate: " + asset.Plate : "SN: " + asset.SerialNo,
                    StatusName = asset.AssetStatus.StatusName,
                    ConditionName = asset.Condition.ConditionName
                })
                .ToListAsync();

            return vwAssets;
        }
    }
}
