using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
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

            var disposals = _db.Disposals.Where(m=>m.EmployeeId== _employeeId).Include(m=>m.DisposalType)
                .ToList();

            dto.Disposals = disposals;
            dto.StoreId = _storeId;

            return View(dto);
        }
    }
}
