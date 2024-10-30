using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MODAMS.ApplicationServices;
using MODAMS.Models.ViewModels.Dto;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class VerificationsController : Controller
    {
        private readonly IVerificationService _verificationService;
        public VerificationsController(IVerificationService verificationService)
        {
            _verificationService = verificationService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _verificationService.GetIndexAsync();
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new VerificationsDTO());
            }
        }
        [HttpGet]
        [Authorize(Roles = "StoreOwner")]
        public async Task<IActionResult> CreateSchedule()
        {
            var result = await _verificationService.GetCreateScheduleAsync();
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new VerificationScheduleCreateDTO());
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "StoreOwner")]
        public async Task<IActionResult> CreateSchedule(VerificationScheduleCreateDTO dto, string teamMembersData)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Error Occurred: " + string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction("CreateSchedule");
            }

            var result = await _verificationService.CreateScheduleAsync(dto, teamMembersData);
            dto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = "Verification Schedule created successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Transaction failed: " + result.ErrorMessage;
                return View(dto);
            }
        }
        [HttpGet]
        [Authorize(Roles = "StoreOwner")]
        public async Task<IActionResult> EditSchedule(int id)
        {
            var result = await _verificationService.GetEditScheduleAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new VerificationScheduleEditDTO());
            }
        }
        [HttpPost]
        [Authorize(Roles = "StoreOwner")]
        public async Task<IActionResult> EditSchedule(VerificationScheduleEditDTO dto, string teamMembersData)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                TempData["error"] = "Validation failed: " + string.Join("; ", errors);
                return View(dto);
            }

            var result = await _verificationService.EditScheduleAsync(dto, teamMembersData);
            dto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = "Schedule updated successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(dto);
            }
        }
        [HttpGet]
        public async Task<IActionResult> PreviewSchedule(int id)
        {
            var result = await _verificationService.GetPreviewScheduleAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new VerificationSchedulePreviewDTO());
            }
        }
        [HttpPost]
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> VerifyAsset(VerificationSchedulePreviewDTO dto, IFormFile file)
        {
            var result = await _verificationService.VerifyAssetAsync(dto, file);

            if (result.IsSuccess)
            {
                return Json(new { success = true, message = "Asset verified successfully" });
            }
            else
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }
        }
        [Authorize(Roles = "StoreOwner")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            if (id == 0)
            {
                TempData["error"] = "Select a schedule to delete!";
                return RedirectToAction("Index");
            }

            var result = await _verificationService.DeleteScheduleAsync(id);

            if (result.IsSuccess)
            {
                TempData["success"] = "Schedule deleted successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Index");
            }
        }
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> DeleteVerificationRecord(int id)
        {
            var result = await _verificationService.DeleteVerificationRecordAsync(id);

            if (result.IsSuccess)
            {
                return Json(new { success = true, message = "Verification Record deleted successfuly!" });
            }
            else
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }
        }
        [Authorize(Roles = "StoreOwner, User")]
        [HttpGet("/Users/Verifications/GetScheduleAssets")]
        public async Task<IActionResult> GetScheduleAssets(int id)
        {
            var result = await _verificationService.GetPreviewScheduleAsync(id);
            if (result.IsSuccess)
            {
                return Json(new { success = true, assets = result.Value.Assets });
            }
            else
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }
        }
        [Authorize(Roles = "StoreOwner, User")]
        [HttpPost]
        public async Task<IActionResult> CompleteVerificationSchedule(int ScheduleId)
        {
            var result = await _verificationService.CompleteVerificationSchedule(ScheduleId);
            if (result.IsSuccess)
            {
                TempData["success"] = "Verification set to completed successfuly!";
                return RedirectToAction("PreviewSchedule", new { id = ScheduleId });
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("PreviewSchedule", new { id = ScheduleId });
            }
        }
    }
}
