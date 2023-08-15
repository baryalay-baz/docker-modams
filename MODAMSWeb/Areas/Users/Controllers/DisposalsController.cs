using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using MODAMS.Models;
using MODAMS.Models.ViewModels.Dto;
using MODAMS.Utility;
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

        public DisposalsController(ApplicationDbContext db, IAMSFunc func)
        {
            _db = db;
            _func = func;
            _employeeId = _func.GetEmployeeId();
        }

        public IActionResult Index()
        {
            _employeeId = User.IsInRole("User") ? _func.GetSupervisorId(_employeeId) : _employeeId;
            _storeId = _func.GetStoreIdByEmployeeId(_employeeId);


            var dto = new dtoDisposal();

            if (_employeeId == _func.GetStoreOwnerId(_employeeId))
                dto.IsAuthorized = true;

            var disposals = _db.Disposals.Where(m=>m.EmployeeId== _employeeId)
                .Include(m=>m.DisposalType).Include(m=>m.Asset)
                .Include(m=>m.Asset.SubCategory).Include(m=>m.Asset.SubCategory.Category)
                .ToList();

            dto.Disposals = disposals;
            dto.StoreId = _storeId;
            dto.IsAuthorized = true;

            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = "StoreOwner, User")]
        public IActionResult CreateDisposal() {
            var dto = new dtoCreateDisposal();
            _employeeId = User.IsInRole("User") ? _func.GetSupervisorId(_employeeId) : _employeeId;
            _storeId = _func.GetStoreIdByEmployeeId(_employeeId);

            var assetList = _db.Assets.Where(m => m.StoreId == _storeId)
                .Include(m=>m.SubCategory).Include(m=>m.SubCategory.Category)
                .ToList();
            
            dto.Assets = assetList;
            dto.IsAuthorized = true;

            var disposalTypeList = _db.DisposalTypes.ToList().Select(m => new SelectListItem
            {
                Text = m.Type,
                Value = m.Id.ToString()
            });
            dto.DisposalTypeList = disposalTypeList;

            return View(dto);
        }
    }
}
