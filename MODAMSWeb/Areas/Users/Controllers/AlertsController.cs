using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Models.ViewModels;
using MODAMS.DataAccess.Data;
using MODAMS.Utility;
using NuGet.ContentModel;
using MODAMS.Models;
using Microsoft.EntityFrameworkCore;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class AlertsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;

        public AlertsController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
        }

        public IActionResult Index()
        {
            var vwAssetDocs = new List<vwAssetDocuments>();
            var assets = _db.Assets.Include(m=>m.SubCategory)
                .Select(m => new { m.Id, m.SubCategoryId, m.Make, m.Model, m.Name, m.SubCategory }).ToList();

            List<vwAlert> Alerts = new List<vwAlert>();

            foreach (var asset in assets)
            {
                var blnCheck = false;
                vwAssetDocs = GetAssetDocuments(asset.Id);
                foreach (var assetDoc in vwAssetDocs)
                {
                    if (assetDoc.Id == 0)
                    {
                        blnCheck = true; break;
                    }
                }
                if (blnCheck)
                {
                    var alert = new vwAlert()
                    {
                        AssetId = asset.Id,
                        SubCategory = asset.SubCategory.SubCategoryName,
                        Make = asset.Make,
                        Model = asset.Model,
                        Name = asset.Name,
                        Alert = "Missing Document"
                    };
                    Alerts.Add(alert);
                }
            }

            return View(Alerts);
        }

        private List<vwAssetDocuments> GetAssetDocuments(int AssetId)
        {
            var documentTypes = _db.DocumentTypes.ToList();
            var vwAssetDocs = new List<vwAssetDocuments>();

            foreach (var documentType in documentTypes)
            {
                var vwDocType = new vwAssetDocuments()
                {
                    Id = GetAssetDocumentId(AssetId, documentType.Id),
                    DocumentTypeId = documentType.Id,
                    Name = documentType.Name,
                    AssetId = AssetId,
                    DocumentUrl = GetDocumentUrl(AssetId, documentType.Id)
                };
                vwAssetDocs.Add(vwDocType);
            }
            return vwAssetDocs;
        }
        private string GetDocumentUrl(int assetId, int documentTypeId)
        {
            var assetDocument = _db.AssetDocuments.Where(m => m.AssetId == assetId && m.DocumentTypeId == documentTypeId).FirstOrDefault();
            if (assetDocument != null)
            {
                return assetDocument.DocumentUrl;
            }
            else
            {
                return "not yet uploaded!";
            }

        }
        private int GetAssetDocumentId(int assetId, int documentTypeId)
        {
            int nResult = 0;
            var assetDocument = _db.AssetDocuments.Where(m => m.AssetId == assetId && m.DocumentTypeId == documentTypeId).FirstOrDefault();
            if (assetDocument != null)
            {
                nResult = assetDocument.Id;
            }
            return nResult;
        }
    }
}
