using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MODAMS.ApplicationServices.IServices;
using MODAMS.Models.ViewModels.Dto;

namespace MODAMSWeb.Areas.Users.Controllers
{
    [Area("Users")]
    [Authorize]
    public class StoresController : Controller
    {
        private readonly IStoreService _storeService;
        public StoresController(IStoreService storeService)
        {
            _storeService = storeService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _storeService.GetIndexAsync();
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
                return View(new StoresDTO());
            }

        }
        [HttpGet]
        public async Task<IActionResult> StoreDetails(int id)
        {
            var result = await _storeService.GetStoreDetailsAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Index","Stores");
            }
        }
    }
}
