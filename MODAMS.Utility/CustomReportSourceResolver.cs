using MODAMS.DataAccess.Data;
using MODAMS.Models.ViewModels;
using System.Collections.Generic;
using System.IO;
using Telerik.Reporting;
using Telerik.Reporting.Services;

namespace MODAMS.Utility
{
    public class CustomReportSourceResolver : IReportSourceResolver
    {
        private string ReportsPath = "/Reports";
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
                //// Set the data source for the report
                report.DataSource = _db.vwAssets;
                //// Set the data source for another data item
                //var graph1 = report.Items.Find("graph1", true)[0] as Graph;
                //graph1.DataSource = _db.vwAssets;
            };
            return new InstanceReportSource
            {
                ReportDocument = report
            };
        }
    }
}
