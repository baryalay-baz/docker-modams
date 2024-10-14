using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.ApplicationServices;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
using NuGet.ContentModel;
using Org.BouncyCastle.Ocsp;

namespace MODAMSWeb.Areas.Users.Controllers
{

    [Area("Users")]
    [Authorize]
    public class DisposalsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;
        private int _storeId;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly IDisposalService _disposalService;

        public DisposalsController(IDisposalService disposalService, ApplicationDbContext db, IAMSFunc func, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
            _webHostEnvironment = webHostEnvironment;
            _disposalService = disposalService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _disposalService.GetIndexAsync();
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return View(new DisposalsDTO());
            }
        }

        [HttpGet]
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> CreateDisposal()
        {
            var result = await _disposalService.GetCreateDisposalAsync();
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return View(new DisposalCreateDTO());
            }
        }

        //Post CreateDisposal form
        [HttpPost]
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> CreateDisposal(DisposalCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill all the mandatory fields!";
                return View(await _disposalService.PopulateDisposalDtoAsync(dto));
            }

            var result = await _disposalService.CreateDisposalAsync(dto);
            var resultDto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = "Disposal added successfully!";
                return RedirectToAction("Index");
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return View(await _disposalService.PopulateDisposalDtoAsync(dto));
            }
        }

        [HttpGet]
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> EditDisposal(int id)
        {
            var result = await _disposalService.GetEditDisposalAsync(id);

            if (result.IsSuccess)
            {
                return View(result.Value);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Disposals");
            }
        }


        [HttpPost]
        [Authorize(Roles = "StoreOwner, User")]
        public async Task<IActionResult> EditDisposal(DisposalEditDTO dto)
        {

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please fill all the mandatory fields!";
                dto = await _disposalService.PopulateDisposalDtoAsync(dto);

                return View(dto);
            }

            var result = await _disposalService.EditDisposalAsync(dto);
            if (result.IsSuccess)
            {
                TempData["success"] = "Disposal updated successfully!";
                return RedirectToAction("EditDisposal", "Disposals", new { dto.Disposal.Id });
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(await _disposalService.PopulateDisposalDtoAsync(dto));
            }
        }


    }
}
