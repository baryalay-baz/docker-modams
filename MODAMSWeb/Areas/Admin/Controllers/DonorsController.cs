using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MODAMS.ApplicationServices.IServices;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Utility;
using System.Data;

namespace MODAMSWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator, SeniorManagement, StoreOwner")]
    public class DonorsController : Controller
    {
        private readonly IDonorService _donorService;
        public DonorsController(IDonorService donorService)
        {
            _donorService = donorService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _donorService.GetIndexAsync();
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new List<Donor>());
            }

        }
        [HttpGet]
        public IActionResult CreateDonor()
        {
            var result = _donorService.GetCreateDonor();
            var dto = result.Value;
            return View(dto);
        }
        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDonor(Donor donor)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "All fields are mandatory!";
                return View(donor);
            }

            var result = await _donorService.CreateDonorAsync(donor);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = "Donor added successfully!";
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
            }
            return RedirectToAction("Index", "Donors");
        }
        [HttpGet]
        public async Task<IActionResult> EditDonor(int id)
        {
            var result = await _donorService.GetEditDonorAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new Donor());
            }
        }
        [Authorize(Roles = "Administrator, StoreOwner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDonor(Donor donor)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "All fields are mandatory!";
                return View(donor);
            }

            var result = await _donorService.EditDonorAsync(donor);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = "Donor added successfully!";
                return RedirectToAction("Index", "Donors");
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return View(dto);
            }

        }
    }
}