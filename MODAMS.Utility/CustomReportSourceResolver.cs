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
        public CustomReportSourceResolver(ApplicationDbContext db , IAMSFunc func)
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
            Report report = null;
            using (var sourceStream = System.IO.File.OpenRead(reportPath))
            {
                report = (Report)reportPackager.UnpackageDocument(sourceStream);
            }
            if (operationOrigin == OperationOrigin.GenerateReportDocument)
            {
                if (reportId == "AssetReport.trdp")
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
                else if (reportId == "TransferVoucher.trdp")
                {
                    var data = GetTransferVoucherData(currentParameterValues).GetAwaiter().GetResult();
                    if (data != null)
                    {
                        report.DataSource = data;
                    }
                }
                else if (reportId == "TransferReport.trdp")
                {
                    var data = GetTransferReport(currentParameterValues).GetAwaiter().GetResult();
                    if (data != null)
                    {
                        report.DataSource = data;
                    }
                }
                else if (reportId == "DisposalReport.trdp")
                {
                    var data = GetDisposalReport(currentParameterValues).GetAwaiter().GetResult();
                    if (data != null)
                    {
                        report.DataSource = data;
                    }
                }
            };
            return new InstanceReportSource
            {
                ReportDocument = report
            };
        }
        public async Task<List<vwDisposal>> GetDisposalReport(IDictionary<string, object> currentParameterValues)
        {
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
                {
                    query = query.Where(m => m.StoreId == Convert.ToInt64(parameter.Value));
                }
                else if (parameter.Key == "DisposalTypeId" && Convert.ToInt64(parameter.Value) > 0)
                {
                    query = query.Where(m => m.DisposalTypeId == Convert.ToInt64(parameter.Value));
                }
            }
            var data = await query.ToListAsync();

            return data;

        }
        public async Task<List<vwTransferVoucher>> GetTransferReport(IDictionary<string, object> currentParameterValues)
        {
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
                TransferNumber = tv.TransferNumber
            });


            foreach (var parameter in currentParameterValues)
            {
                if (parameter.Key == "StoreFromId" && Convert.ToInt64(parameter.Value) > 0)
                {
                    query = query.Where(m => m.StoreFromId == Convert.ToInt64(parameter.Value));
                }
                else if (parameter.Key == "StoreToId" && Convert.ToInt64(parameter.Value) > 0)
                {
                    query = query.Where(m => m.StoreToId == Convert.ToInt64(parameter.Value));
                }
                else if (parameter.Key == "TransferStatusId" && Convert.ToInt64(parameter.Value) > 0)
                {
                    query = query.Where(m => m.TransferStatusId == Convert.ToInt64(parameter.Value));
                }
            }
            var data = await query.ToListAsync();

            return data;

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
                // Add more conditions for other parameters if needed
            }

            if (storeId == 0)
            {
                var storeList = await _func.GetStoresByEmployeeIdAsync(_employeeId);

                var storeIds = storeList.Select(s => s.Id).ToList();
                query = query.Where(asset => storeIds.Contains(asset.StoreId));
            }

            var vwAssets = await query.ToListAsync();

            return vwAssets;
        }
    }
}
