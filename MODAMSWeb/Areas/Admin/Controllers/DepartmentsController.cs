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

namespace MODAMSWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator, SeniorManagement, StoreOwner")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAMSFunc _func;
        private int _employeeId;

        public DepartmentsController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();

        }

        public IActionResult Index()
        {
            List<vwDepartments> departments = _db.vwDepartments.ToList();
            return View(departments);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult CreateDepartment()
        {
            var departmentDto = new dtoDepartment();

            departmentDto.department = new Department();

            var employeeList = _db.Employees.ToList().Select(m => new SelectListItem
            {
                Text = m.FullName,
                Value = m.Id.ToString()
            });

            var departmentList = _db.vwDepartments.ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            });

            departmentDto.Employees = employeeList;
            departmentDto.Departments = departmentList;

            return View(departmentDto);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(dtoDepartment form)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "All fields are mandatory";
                return View(form);
            }

            var department = await _db.Departments
                .Where(m => m.Name == form.department.Name).FirstOrDefaultAsync();

            if (department == null)
            {
                await _db.Departments.AddAsync(form.department);
                await _db.SaveChangesAsync();
                await CreateStore(form.department);

                TempData["success"] = "Department created succcessfuly!";
            }
            else
            {
                TempData["error"] = "Department already exists!";
                return View(form);
            }

            return RedirectToAction("Index", "Departments");
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult EditDepartment(int id)
        {
            if (id == 0)
            {
                TempData["error"] = "Please select a department!";
                return RedirectToAction("Index", "Departments");
            }

            var department = _db.Departments.Where(m => m.Id == id).FirstOrDefault();
            string departmentOwner = "";

            if (department == null)
            {
                TempData["error"] = "Department not found!";
                return RedirectToAction("Index", "Departments");
            }
            else {
                departmentOwner = _func.GetEmployeeNameById(department.EmployeeId == 0 || department.EmployeeId == null ? 0 : (int)department.EmployeeId);
            }

            var employeeList = _db.Employees.ToList().Select(m => new SelectListItem
            {
                Text = m.FullName,
                Value = m.Id.ToString()
            });

            var departmentList = _db.vwDepartments.ToList().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            });

            var dto = new dtoDepartment()
            {
                department = department,
                Employees = employeeList,
                Departments = departmentList,
                DepartmentOwner = departmentOwner
            };

            return View(dto);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(dtoDepartment form)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "All fields are mandatory!";
                return View(form);
            }
            var department = _db.Departments.Where(m => m.Id == form.department.Id).FirstOrDefault();
            if (department == null)
            {
                TempData["error"] = "Department not found!";
                return View(form);
            }
            department.Name = form.department.Name;
            department.UpperLevelDeptId = form.department.UpperLevelDeptId;
            
            await _db.SaveChangesAsync();

            TempData["success"] = "Department saved successfuly!";
            return RedirectToAction("Index", "Departments");
        }

        [HttpGet]
        [Authorize(Roles = "SeniorManagement, Administrator, StoreOwner")]
        public IActionResult OrganizationChart()
        {
            var departments = _db.vwDepartments.ToList().OrderBy(m => m.Id);

            List<dtoOrganizationChart> dto = new List<dtoOrganizationChart>();
            int nCounter = 0;
            foreach (var item in departments)
            {
                nCounter++;
                string imageUrl = (item.ImageUrl == string.Empty) ? "/assets/images/faces/profile_placeholder.png" : item.ImageUrl;
                int? parentId = (item.UpperLevelDeptId == 0) ? null : item.UpperLevelDeptId;

                dto.Add(new dtoOrganizationChart()
                {
                    ID = item.Id,
                    Name = item.Name,
                    ParentID = parentId,
                    Title = item.OwnerName,
                    Avatar = imageUrl,
                    Expanded = false
                });

            }
            return View(dto);
        }

        public JsonResult Read([DataSourceRequest] DataSourceRequest request)
        {
            var departments = _db.vwDepartments.ToList().OrderBy(m => m.Id);

            List<dtoOrganizationChart> dto = new List<dtoOrganizationChart>();
            int nCounter = 0;
            foreach (var item in departments)
            {
                nCounter++;
                string imageUrl = (item.ImageUrl == string.Empty) ? "/assets/images/faces/profile_placeholder.png" : item.ImageUrl;
                int? parentId = (item.UpperLevelDeptId == 0) ? null : item.UpperLevelDeptId;

                dto.Add(new dtoOrganizationChart()
                {
                    ID = item.Id,
                    Name = item.Name,
                    ParentID = parentId,
                    Title = item.OwnerName,
                    Avatar = imageUrl,
                    Expanded = false
                });

            }
            return Json(dto);
        }


        [HttpGet]
        [Authorize(Roles = "SeniorManagement, Administrator, StoreOwner")]
        public string GetDepartments()
        {
            string sResult = "No Records Found";
            var departments = _db.vwDepartments.ToList();
            if (departments != null)
            {
                sResult = JsonConvert.SerializeObject(departments);
            }
            return sResult;
        }

        [HttpGet]
        [Authorize(Roles = "SeniorManagement, Administrator, StoreOwner")]
        public IActionResult DepartmentHeads(int id)
        {

            List<DepartmentHead> departmentHeads = _db.DepartmentHeads.Where(m => m.DepartmentId == id)
                .Include(m => m.Employee).Include(m => m.Department).OrderByDescending(m => m.StartDate).ToList();

            var employeeList = _db.vwAvailableEmployees.ToList().Select(m => new SelectListItem
            {
                Text = m.FullName,
                Value = m.Id.ToString()
            });
            var storeOwner = _func.GetEmployeeNameById(_func.GetDepartmentHead(id));
            storeOwner = storeOwner == "Not found!" ? "Vacant" : storeOwner;
            var dto = new dtoDepartmentHeads()
            {
                DepartmentHeads = departmentHeads,
                Employees = employeeList,
                DepartmentId = id,
                DepartmentName = _func.GetDepartmentNameById(id),
                Owner = storeOwner
            };

            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AssignOwner(dtoDepartmentHeads dto)
        {
            int nDepartmentId = dto.DepartmentId;
            int nEmployeeId = dto.EmployeeId;

            var departmentHead = await _db.DepartmentHeads
                .FirstOrDefaultAsync(m => m.DepartmentId == nDepartmentId && m.IsActive);

            if (departmentHead != null)
            {
                departmentHead.IsActive = false;
                departmentHead.EndDate = DateTime.Now;
            }

            if (nEmployeeId != 0)
            {
                var newDepartmentHead = new DepartmentHead
                {
                    DepartmentId = nDepartmentId,
                    EmployeeId = nEmployeeId,
                    StartDate = DateTime.Now,
                    IsActive = true
                };
                _db.DepartmentHeads.Add(newDepartmentHead);
            }

            var department = await _db.Departments.FirstOrDefaultAsync(m => m.Id == nDepartmentId);
            if (department != null)
            {
                department.EmployeeId = nEmployeeId != 0 ? nEmployeeId : null;
            }

            await _db.SaveChangesAsync();

            TempData["success"] = "Owner set successfully!";
            return RedirectToAction("Index", "Departments");
        }



        //private functions
        private async Task CreateStore(Department department)
        {
            var store = new Store()
            {
                Name = department.Name,
                Description = "Store for " + department.Name,
                DepartmentId = department.Id
            };
            await _db.Stores.AddAsync(store);
            await _db.SaveChangesAsync();
        }
    }
}