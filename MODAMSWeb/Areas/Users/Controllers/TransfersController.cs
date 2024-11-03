using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.ApplicationServices.IServices;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Notification = MODAMS.Models.Notification;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class TransfersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private readonly ITransferService _transferService;

        private int _employeeId;
        private int _supervisorId;
        private int _storeId;
        public TransfersController(ApplicationDbContext db, IAMSFunc func, ITransferService transferService)
        {
            _db = db;
            _func = func;
            _transferService = transferService;

            _employeeId = _func.GetEmployeeId();
            _supervisorId = _func.GetSupervisorId(_employeeId);
        }
        [HttpGet]
        public async Task<IActionResult> Index(int id = 0, int transferStatusId = 0)
        {
            var result = await _transferService.GetIndexAsync(id, transferStatusId);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return View(new TransferDTO());
            }

        }
        [Authorize(Roles = "StoreOwner, User")]
        [HttpGet]
        public async Task<IActionResult> CreateTransfer()
        {
            var result = await _transferService.GetCreateTransferAsync();
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return View(new TransferDTO());
            }
        }
        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTransfer(TransferCreateDTO transferDTO, string selectedAssets)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill all the mandatory fields";
                return RedirectToAction("CreateTransfer", "Transfers");
            }

            var result = await _transferService.CreateTransferAsync(transferDTO, selectedAssets);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = "Transfer created successfuly!";
                return RedirectToAction("PreviewTransfer", "Transfers", new { id = dto.Transfer.Id });
            }
            else {
                TempData["error"] = "Transaction failed: " + result.ErrorMessage;
                return RedirectToAction("CreateTransfer", "Transfers");
            }
        }
        [Authorize(Roles = "StoreOwner, User")]
        [HttpGet]
        public async Task<IActionResult> EditTransfer(int id)
        {
            var result = await _transferService.GetEditTransferAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Transfers");
            }
        }
        [Authorize(Roles = "StoreOwner, User")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> EditTransfer(TransferEditDTO transferDTO, string selectedAssets)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill all the mandatory fields";
                return RedirectToAction("EditTransfer", "Transfers", new { id = transferDTO.Transfer.Id });
            }

            var result = await _transferService.EditTransferAsync(transferDTO, selectedAssets);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = "Changes saved successfuly!";
            }
            else {
                TempData["error"] = result.ErrorMessage;
            }

            return RedirectToAction("EditTransfer", "Transfers", new { id = dto.Transfer.Id });
        }
        [HttpGet]
        public async Task<IActionResult> PreviewTransfer(int id)
        {
            var result = await _transferService.GetPreviewTransferAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return View(new TransferPreviewDTO());
            }
        }
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> DeleteTransfer(int id)
        {
            var result = await _transferService.DeleteTransferAsync(id);

            if (result.IsSuccess)
            {
                TempData["success"] = "Transfer deleted successfuly!";
                return RedirectToAction("Index", "Transfers");
            }
            else {
                TempData["error"] = result.ErrorMessage;
            }

            var transferToDelete = await _db.Transfers.FirstOrDefaultAsync(m => m.Id == id);

            if (transferToDelete == null)
            {
                return RedirectToAction("Index", "Transfers");
            }

            try
            {
                _db.Transfers.Remove(transferToDelete);
                _db.SaveChanges();
                TempData["success"] = $"Transfer: {transferToDelete.TransferNumber} deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("EditTransfer", "Transfers", new { id });
            }

            return RedirectToAction("Index", "Transfers");
        }
        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForAcknowledgement(int id)
        {
            var result = await _transferService.SubmitForAcknowledgementAsync(id);

            if (result.IsSuccess)
            {
                TempData["success"] = "Transfer Submitted for Acknowledgement";
                return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
            }
        }
        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcknowledgeTransfer(int id)
        {
            var result = await _transferService.AcknowledgeTransferAsync(id);

            if (result.IsSuccess)
            {
                TempData["success"] = "Transfer Acknowledged";
                return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
            }
        }
        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectTransfer(int id, string txtReason = "")
        {
            var result = await _transferService.RejectTransferAsync(id, txtReason);

            if (result.IsSuccess)
            {
                TempData["success"] = "Transfer Rejected Successfuly!";
            }
            else {
                TempData["error"] = result.ErrorMessage;
            }
            return RedirectToAction("PreviewTransfer", "Transfers", new { id = id });
        }
        public async Task<List<TransferChartDTO>> GetChartData(int type)
        {
            var dto = await _transferService.GetChartDataAsync(type);
            return dto;          
        }
        [HttpGet]
        public async Task<IActionResult> TransferredAssets(int id)
        {
            var result = await _transferService.GetTransferredAssetsAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return View(new List<TransfersOutgoingAssetDTO>());
            }
        }
        [HttpGet]
        public async Task<IActionResult> ReceivedAssets(int id)
        {
            var result = await _transferService.GetReceivedAssetsAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return View(new List<List<TransfersIncomingAssetDTO>>());
            }
        }
        [HttpPost]
        public IActionResult PrintVoucher(int id)
        {
            return RedirectToAction("ReportViewerExternal", "Reporting", new { Type = "PrintVoucher", id = id }); // Redirect to a different action after processing the form
        }
    }
}
