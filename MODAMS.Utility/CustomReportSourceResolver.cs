using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using System;
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
        private IAMSFunc _func;

        private int _employeeId;
        private int _supervisorEmployeeId;
        public CustomReportSourceResolver(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _supervisorEmployeeId = _func.GetSupervisorId(_employeeId);
        }

        public ReportSource Resolve(string reportId, OperationOrigin operationOrigin, IDictionary<string, object> currentParameterValues)
        {
            string reportPath = Path.Combine(this.ReportsPath, reportId);
            var reportPackager = new ReportPackager();
            Report report = new Report();
            
            using (var sourceStream = System.IO.File.OpenRead(reportPath))
            {
                report = (Report)reportPackager.UnpackageDocument(sourceStream);
            }
            if (operationOrigin == OperationOrigin.GenerateReportDocument)
            {
                if (reportId == "AssetReport.trdp" || reportId == "AssetReportSo.trdp")
                {
                    // Set the data source for the report
                    var vwAssets = GetAssetList(currentParameterValues).GetAwaiter().GetResult();

                    if (vwAssets != null)
                    {
                        report.DataSource = vwAssets;
                        // Set the data source for another data item
                        var graph1 = report.Items.Find("graph1", true)[0] as Graph;
                        graph1.DataSource = vwAssets;
                    }
                }
                else if (reportId == "TransferVoucher.trdp" || reportId == "TransferVoucherSo.trdp")
                {
                    var data = GetTransferVoucherData(currentParameterValues).GetAwaiter().GetResult();
                    if (data != null)
                    {
                        report.DataSource = data;
                    }
                }
                else if (reportId == "TransferReport.trdp" || reportId == "TransferReportSo.trdp")
                {
                    var data = GetTransferReport(currentParameterValues).GetAwaiter().GetResult();
                    if (data != null)
                    {
                        report.DataSource = data;
                    }
                }
                else if (reportId == "DisposalReport.trdp" || reportId == "DisposalReportSo.trdp")
                {
                    var data = GetDisposalReport(currentParameterValues).GetAwaiter().GetResult();
                    if (data != null)
                    {
                        report.DataSource = data;
                    }
                }
            }
            ;
            return new InstanceReportSource
            {
                ReportDocument = report
            };
        }
        public async Task<List<vwDisposal>> GetDisposalReport(IDictionary<string, object> currentParameterValues)
        {
            var (from, toEx) = ParseDateRange(currentParameterValues);

            IQueryable<vwDisposal> query = _db.vwDisposals.Select(m => new vwDisposal
            {
                Id = m.Id,
                Cost = m.Cost,
                DepartmentName = m.DepartmentName,
                DisposalDate = m.DisposalDate,
                DisposalTypeId = m.DisposalTypeId,
                Identification = m.Identification,
                Name = m.Name,
                StoreId = m.StoreId,
                SubCategoryName = m.SubCategoryName,
                Type = m.Type
            });

            foreach (var parameter in currentParameterValues)
            {
                if (parameter.Key == "StoreId" && Convert.ToInt64(parameter.Value) > 0)
                    query = query.Where(m => m.StoreId == Convert.ToInt64(parameter.Value));
                else if (parameter.Key == "DisposalTypeId" && Convert.ToInt64(parameter.Value) > 0)
                    query = query.Where(m => m.DisposalTypeId == Convert.ToInt64(parameter.Value));
            }

            if (from.HasValue) query = query.Where(m => m.DisposalDate >= from.Value);
            if (toEx.HasValue) query = query.Where(m => m.DisposalDate < toEx.Value);

            return await query.ToListAsync();
        }
        public async Task<List<vwTransferVoucher>> GetTransferReport(IDictionary<string, object> currentParameterValues)
        {
            var (from, toEx) = ParseDateRange(currentParameterValues);

            IQueryable<vwTransferVoucher> query = _db.vwTransferVouchers.Select(tv => new vwTransferVoucher
            {
                Id = tv.Id,
                AcknowledgementDate = tv.AcknowledgementDate,
                AssetId = tv.AssetId,
                Barcode = tv.Barcode,
                ConditionName = tv.ConditionName,
                Cost = tv.Cost,
                Identification = tv.Identification,
                Make = tv.Make,
                Model = tv.Model,
                Name = tv.Name,
                NumberOfAssets = tv.NumberOfAssets,
                SerialNo = tv.SerialNo,
                Status = tv.Status,
                StoreFrom = tv.StoreFrom,
                StoreFromId = tv.StoreFromId,
                StoreTo = tv.StoreTo,
                StoreToId = tv.StoreToId,
                SubCategoryName = tv.SubCategoryName,
                SubmissionForAcknowledgementDate = tv.SubmissionForAcknowledgementDate,
                TransferStatusId = tv.TransferStatusId,
                TransferDate = tv.TransferDate,
                TransferNumber = tv.TransferNumber,
                ConditionNameSo = tv.ConditionNameSo,
                StatusSo = tv.StatusSo,
                SubCategoryNameSo = tv.SubCategoryNameSo,
                StoreFromSo = tv.StoreFromSo,
                StoreToSo = tv.StoreToSo,
                TransferId = tv.TransferId
            });

            foreach (var parameter in currentParameterValues)
            {
                if (parameter.Key == "StoreFromId" && Convert.ToInt64(parameter.Value) > 0)
                    query = query.Where(m => m.StoreFromId == Convert.ToInt64(parameter.Value));
                else if (parameter.Key == "StoreToId" && Convert.ToInt64(parameter.Value) > 0)
                    query = query.Where(m => m.StoreToId == Convert.ToInt64(parameter.Value));
                else if (parameter.Key == "TransferStatusId" && Convert.ToInt64(parameter.Value) > 0)
                    query = query.Where(m => m.TransferStatusId == Convert.ToInt64(parameter.Value));
            }

            if (from.HasValue) query = query.Where(m => m.TransferDate >= from.Value);
            if (toEx.HasValue) query = query.Where(m => m.TransferDate < toEx.Value);

            return await query.ToListAsync();
        }
        public async Task<List<vwTransferVoucher>> GetTransferVoucherData(IDictionary<string, object> currentParameterValues)
        {
            var data = await _db.vwTransferVouchers.ToListAsync();

            foreach (var parameter in currentParameterValues)
            {
                if (parameter.Key == "TransferId" && Convert.ToInt64(parameter.Value) > 0)
                {
                    data = data.Where(m => m.TransferId == Convert.ToInt64(parameter.Value)).ToList();
                }
            }
            return data;
        }
        public async Task<List<vwAsset>> GetAssetList(IDictionary<string, object> currentParameterValues)
        {
            var (from, toEx) = ParseDateRange(currentParameterValues);

            IQueryable<vwAsset> query = _db.Assets
                .Where(m => m.AssetStatusId != SD.Asset_Deleted)
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
                    ManufacturingCountry = asset.ManufacturingCountry ?? string.Empty,
                    SerialNo = asset.SerialNo,
                    BarCode = asset.Barcode ?? string.Empty,
                    Engine = asset.Engine,
                    Chasis = asset.Chasis,
                    Plate = asset.Plate,
                    Specifications = asset.Specifications,
                    Cost = asset.Cost,
                    PurchaseDate = asset.PurchaseDate,
                    PONumber = asset.PONumber ?? string.Empty,
                    RecieptDate = asset.RecieptDate,
                    ProcuredBy = asset.ProcuredBy ?? string.Empty,
                    Remarks = asset.Remarks ?? string.Empty,
                    ConditionId = asset.ConditionId,
                    StoreId = asset.StoreId,
                    AssetStatusId = asset.AssetStatusId,
                    DonorId = asset.DonorId,
                    DonorName = asset.Donor.Name,
                    DepartmentId = asset.Store.DepartmentId,
                    DepartmentName = asset.Store.Department.Name,
                    EmployeeId = asset.Store.Department.EmployeeId,
                    StoreOwner = "-",
                    Identification = asset.SubCategory.Category.CategoryName == "Vehicles" ? _func.BuildVehicleIdentification(asset.Plate,asset.Chasis,asset.Engine) : "SN: " + asset.SerialNo,
                    StatusName = asset.AssetStatus.StatusName,
                    ConditionName = asset.Condition.ConditionName,
                    ConditionNameSo = asset.Condition.ConditionNameSo,
                    StatusNameSo = asset.AssetStatus.StatusNameSo,
                    CategoryNameSo = asset.SubCategory.Category.CategoryNameSo,
                    DepartmentNameSo = asset.Store.Department.NameSo,
                    SubCategoryNameSo = asset.SubCategory.SubCategoryNameSo
                });

            int storeId = 0;
            foreach (var parameter in currentParameterValues)
            {
                if (parameter.Key == "StoreId" && parameter.Value != null)
                {
                    storeId = Convert.ToInt32(parameter.Value);
                    query = query.Where(asset => asset.StoreId == Convert.ToInt64(parameter.Value));
                }
                else if (parameter.Key == "AssetStatusId" && parameter.Value != null)
                {
                    query = query.Where(asset => asset.AssetStatusId == Convert.ToInt64(parameter.Value));
                }
                else if (parameter.Key == "CategoryId" && parameter.Value != null)
                {
                    query = query.Where(asset => asset.CategoryId == Convert.ToInt64(parameter.Value));
                }
                else if (parameter.Key == "ConditionId" && parameter.Value != null)
                {
                    query = query.Where(asset => asset.ConditionId == Convert.ToInt64(parameter.Value));
                }
                else if (parameter.Key == "DonorId" && parameter.Value != null)
                {
                    query = query.Where(asset => asset.DonorId == Convert.ToInt64(parameter.Value));
                }
            }

            // Date range on PurchaseDate (nullable-safe if your column is nullable)
            if (from.HasValue) query = query.Where(asset => asset.PurchaseDate >= from.Value);
            if (toEx.HasValue) query = query.Where(asset => asset.PurchaseDate < toEx.Value);

            if (storeId == 0)
            {
                var storeList = await _func.GetStoresByEmployeeIdAsync(_employeeId);
                var storeIds = storeList.Select(s => s.Id).ToList();
                query = query.Where(asset => storeIds.Contains(asset.StoreId));
            }

            return await query.ToListAsync();
        }
        private static (DateTime? from, DateTime? toExclusive) ParseDateRange(IDictionary<string, object> p)
        {
            DateTime? from = null, toExclusive = null;

            DateTime parseBoxed(object v)
            {
                if (v is DateTime dt) return dt;
                var s = Convert.ToString(v)?.Trim();
                // Accept ISO first, then a few common fallbacks
                var formats = new[] { "yyyy-MM-dd", "dd-MMM-yyyy", "dd/MM/yyyy", "MM/dd/yyyy" };
                if (DateTime.TryParseExact(s, formats, System.Globalization.CultureInfo.InvariantCulture,
                                           System.Globalization.DateTimeStyles.AssumeLocal, out var parsed))
                    return parsed;
                // Last resort
                return DateTime.Parse(s);
            }

            if (p.TryGetValue("DateFrom", out var df) && df is not null && !string.IsNullOrWhiteSpace(df.ToString()))
                from = parseBoxed(df).Date;

            if (p.TryGetValue("DateTo", out var dt) && dt is not null && !string.IsNullOrWhiteSpace(dt.ToString()))
                // exclusive end to make the range inclusive by day without using .Date in the query
                toExclusive = parseBoxed(dt).Date.AddDays(1);

            return (from, toExclusive);
        }
    }
}
