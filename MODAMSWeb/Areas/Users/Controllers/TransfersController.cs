using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using System.Security.Policy;
using Telerik.SvgIcons;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize(Roles = "StoreOwner, SeniorManagement, Administrator")]
    public class TransfersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;

        public TransfersController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "StoreOwner, User")]
        [HttpGet]
        public IActionResult CreateTransfer()
        {
            var dto = new dtoTransfer();
            List<Asset> assets = GetAssets().GetAwaiter().GetResult();
            List<dtoTransferAsset> transferAssets = new List<dtoTransferAsset>();

            dto.Transfer.TransferNumber = GetTransferNumber();

            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    var transferAsset = new dtoTransferAsset()
                    {
                        AssetId = asset.Id,
                        AssetName = asset.Name,
                        Category = asset.SubCategory.Category.CategoryName,
                        SubCategory = asset.SubCategory.SubCategoryName,
                        Make = asset.Make,
                        Model = asset.Model,
                        Barcode = asset.Barcode.ToString(),
                        SerialNumber = asset.SerialNo,
                        IsSelected = false
                    };
                    transferAssets.Add(transferAsset);
                }
            }
            if (transferAssets.Count > 0)
            {
                dto.Assets = transferAssets;
            }
            int currentStoreId = GetCurrentStoreId();

            var storeList = _db.Stores.Where(m => m.Id != currentStoreId).ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString(),
            });

            dto.StoreList = storeList;

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTransfer(dtoTransfer transferDTO)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill all the mandatory fields";
                return RedirectToAction("CreateTransfer", "Transfers");
            }
            var transfer = new Transfer()
            {
                TransferDate = transferDTO.Transfer.TransferDate,
                EmployeeId = _employeeId,
                StoreId = transferDTO.Transfer.StoreId,
                TransferNumber = transferDTO.Transfer.TransferNumber,
                TransferStatusId = 1,
                Notes = transferDTO.Transfer.Notes,
                SubmissionForAcknowledgementDate = DateTime.Now
            };

            try {
                _db.Transfers.Add(transfer);
                _db.SaveChanges();
            } catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("CreateTransfer", "Transfers");
            }

            var assets = transferDTO.Assets.Where(m => m.IsSelected == true).ToList();
            foreach (var asset in assets)
            {
                var transferDetail = new TransferDetail()
                {
                    AssetId = asset.AssetId,
                    PrevStoreId = GetCurrentStoreId(),
                    TransferId = transfer.Id
                };
                _db.TransferDetails.Add(transferDetail);
            }
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex) {
                TempData["error"] = ex.Message;
                return RedirectToAction("CreateTransfer", "Transfers");
            }

            TempData["success"] = "Transfer saved and submitted for acknowledgement";
            return RedirectToAction("CreateTransfer", "Transfers");
        }
        private string GetTransferNumber()
        {
            string sResult = "";
            int nEmployeeId = _employeeId;
            if (User.IsInRole("User"))
            {
                nEmployeeId = _func.GetSupervisorId(_employeeId);
            }
            TempData["storeFrom"] = _func.GetDepartmentName(nEmployeeId);
            var transfers = _db.Transfers.Where(m => m.EmployeeId == nEmployeeId).ToList();
            var maxIdTransfer = transfers.OrderByDescending(m => m.Id).FirstOrDefault();

            int maxId = 0;
            if (maxIdTransfer != null)
            {
                maxId = maxIdTransfer.Id;
            };
            maxId++;

            if (maxId < 10)
            {
                sResult = "0000" + maxId;
            }
            else if (maxId > 10 && maxId < 100)
            {
                sResult = "000" + maxId;
            }
            else if (maxId > 100 && maxId < 1000)
            {
                sResult = "00" + maxId;
            }
            else if (maxId > 1000 && maxId < 10000)
            {
                sResult = "0" + maxId;
            }
            else if (maxId > 10000 && maxId < 100000)
            {
                sResult = maxId.ToString();
            }
            else
            {
                sResult = maxId.ToString();
            }

            return sResult;
        }
        private async Task<List<Asset>> GetAssets()
        {
            int nEmployeeId = _employeeId;
            if (User.IsInRole("User"))
            {
                nEmployeeId = _func.GetSupervisorId(_employeeId);
            }
            var assets = await _db.Assets.Include(m => m.Store.Department)
                .Include(m => m.SubCategory).Include(m => m.SubCategory.Category)
                .Where(m => m.AssetStatusId == 1).ToListAsync();
            assets = assets.Where(m => m.Store.Department.EmployeeId == nEmployeeId).ToList();
            return assets;
        }

        private int GetCurrentStoreId()
        {
            int nEmployeeId = _employeeId;
            if (User.IsInRole("Users"))
            {
                nEmployeeId = _func.GetSupervisorId(_employeeId);
            }
            int currentStoreId = _db.Stores.Where(m => m.DepartmentId == _func.GetDepartmentId(_employeeId))
                .Select(m => m.Id).FirstOrDefault();

            return currentStoreId;
        }
    }
}
