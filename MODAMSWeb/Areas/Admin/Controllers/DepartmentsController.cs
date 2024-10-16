using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MODAMS.DataAccess.Data;
using MODAMS.Utility;
using Microsoft.EntityFrameworkCore;
using MODAMS.Models.ViewModels;
using MODAMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Kendo.Mvc.UI;

using MODAMS.Models.ViewModels.Dto;
using MODAMS.ApplicationServices;

namespace MODAMSWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator, SeniorManagement, StoreOwner")]
    public class DepartmentsController : Controller
    {
        private readonly IDepartmentsService _departmentsService;
        public DepartmentsController(IDepartmentsService departmentsService)
        {
            _departmentsService = departmentsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _departmentsService.GetIndexAsync();
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return View(new DepartmentsDTO());
            }
        }
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateDepartment()
        {
            var result = await _departmentsService.GetCreateDepartmentAsync();
            var dto = result.Value;

            if (result.IsSuccess) {
                return View(dto);
            }

            TempData["error"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(DepartmentDTO form)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "All fields are mandatory";
                form = await _departmentsService.PopulateDepartmentDTO(form);
                return View(form);
            }
            
            var result = await _departmentsService.CreateDepartmentAsync(form);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = "Department created succcessfuly!";
                return RedirectToAction("Index", "Departments");
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return View(dto);
            }
        }       
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditDepartment(int id)
        {
            var result = await _departmentsService.GetEditDepartmentAsync(id);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Departments");
            }
        }
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(DepartmentDTO form)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "All fields are mandatory!";
                return View(form);
            }
            var result = await _departmentsService.EditDepartmentAsync(form);
            var dto = result.Value;

            if (result.IsSuccess)
            {
                TempData["success"] = "Department updated successfully!";
                return RedirectToAction("Index", "Departments");
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return View(dto);
            }
        }
        [HttpGet]
        [Authorize(Roles = "SeniorManagement, Administrator, StoreOwner")]
        public async Task<IActionResult> OrganizationChart()
        {
            var result = await _departmentsService.GetOrganizationChartAsync();
            var dto = result.Value;

            if (result.IsSuccess)
            {
                return View(dto);
            }

            TempData["error"] = result.ErrorMessage;
            return View(new List<OrganizationChartDTO>());
        }
        public async Task<JsonResult> Read([DataSourceRequest] DataSourceRequest request)
        {
            var dto = await _departmentsService.GetOrganizationChartJsonAsync();
            return Json(dto);
        }
        [HttpGet]
        [Authorize(Roles = "SeniorManagement, Administrator, StoreOwner")]
        public async Task<string> GetDepartments()
        {
            return await _departmentsService.GetDepartmentsAsync();
        }
        [HttpGet]
        [Authorize(Roles = "SeniorManagement, Administrator, StoreOwner")]
        public async Task<IActionResult> DepartmentHeads(int id)
        {
            var result = await _departmentsService.GetDepartmentHeadsAsync(id);
            var dto = result.Value;

            if (result.IsSuccess) {
                return View(dto);
            }

            TempData["error"] = result.ErrorMessage;
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AssignOwner(DepartmentHeadsDTO dto)
        {
            var result = await _departmentsService.AssignOwnerAsync(dto);

            if (result.IsSuccess)
            {
                TempData["success"] = "Owner set successfully!";
                return RedirectToAction("Index", "Departments");
            }
            else {
                TempData["error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Departments");
            }
        }
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> VacateDepartment(DepartmentHeadsDTO dto)
        {
            var result = await _departmentsService.VacateDepartmentAsync(dto);
            if (result.IsSuccess)
            {
                TempData["success"] = "Store Vacated Successfuly!";
                return RedirectToAction("Index", "Departments");
            }

            TempData["error"] = result.ErrorMessage;
            return RedirectToAction("Index", "Departments");
        }
    }
}