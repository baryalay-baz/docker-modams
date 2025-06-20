﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MODAMS.ApplicationServices.IServices;
using MODAMS.Models.ViewModels.Dto;

namespace MODAMSWeb.Areas.Admin.Controllers

{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class SettingsController : Controller
    {
        private readonly ISettingsService _settingsService;
        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }
        public async Task<IActionResult> Index(int? nMonth, int? nYear)
        {
            var result = await _settingsService.GetIndexAsync(nMonth, nYear);

            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new SettingsDTO());
            }
        }
        public async Task<IActionResult> SetStoreEmployees() 
        {
            var result = await _settingsService.SetStoreEmployeesAsync();
            var dto = result.Value;
            if (result.IsSuccess)
            {
                TempData["success"] = $"{dto} employees updated successfully.";
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
            }
            return RedirectToAction("Index");
        }
    }
}
