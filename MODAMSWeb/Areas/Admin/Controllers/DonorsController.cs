using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;

        public DonorsController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
        }

        public IActionResult Index()
        {
            var donors = _db.Donors.ToList();
            return View(donors);
        }

        [HttpGet]
        public IActionResult CreateDonor()
        {
            Donor donor = new Donor();
            return View(donor);
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

            var donorInDb = await _db.Donors.Where(m => m.Code == donor.Code).FirstOrDefaultAsync();
            if (donorInDb != null)
            {
                TempData["error"] = "Donor with this code already exists!";
                return View(donor);
            }

            await _db.Donors.AddAsync(donor);
            await _db.SaveChangesAsync();

            TempData["success"] = "Donor added successfuly!";
            return RedirectToAction("Index", "Donors");
        }

        [HttpGet]
        public IActionResult EditDonor(int id)
        {
            if (id == 0) {
                TempData["error"] = "Please select a donor";
                return RedirectToAction("Index", "Donors");
            }

            var donor = _db.Donors.Where(m=>m.Id == id).FirstOrDefault();
            if (donor != null)
            {
                return View(donor);
            }
            else {
                TempData["error"] = "Donor not found!";
                return RedirectToAction("Index", "Donors");
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

            var donorInDb = await _db.Donors.Where(m => m.Id == donor.Id).FirstOrDefaultAsync();
            if (donorInDb != null)
            {
                donorInDb.Code = donor.Code;
                donorInDb.Name = donor.Name;
                await _db.SaveChangesAsync();
            }
            else {
                TempData["error"] = "Donor not found!";
                return RedirectToAction("Index", "Donors");
            }
            

            TempData["success"] = "Donor saved successfuly!";
            return RedirectToAction("Index", "Donors");
        }

    }
}
