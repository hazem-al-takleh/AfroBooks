using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AfroBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        // GET: CoverTypes

        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<CoverType> coverTypesList = _unitOfWork.CoverTypes.GetAll();
            return View(coverTypesList);
        }

        // GET
        public IActionResult Create()
        {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType coverType)
        {
            if (!ModelState.IsValid)
                return View(coverType);

            _unitOfWork.CoverTypes.Add(coverType);
            _unitOfWork.Save();
            TempData["dbmsg"] = "Cover Type Created successfully";
            return RedirectToAction("Index");

        }

        // GET
        public IActionResult Update(int? id)
        {
            if (id is null)
                return NotFound();
            var coverType = _unitOfWork.CoverTypes.GetFirstOrDefault(u => u.Id == id);
            if (coverType is null)
                return NotFound();
            return View(coverType);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(CoverType coverType)
        {
            if (!ModelState.IsValid)
                return View(coverType);
            _unitOfWork.CoverTypes.Update(coverType);
            _unitOfWork.Save();
            TempData["dbmsg"] = "Cover Type Updated successfully";
            return RedirectToAction("Index");

        }

        // GET
        public IActionResult Delete(int? id)
        {
            if (id is null)
                return NotFound();
            var coverType = _unitOfWork.CoverTypes.GetFirstOrDefault(u => u.Id == id);
            if (coverType is null)
                return NotFound();
            return View(coverType);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(CoverType coverType)
        {
            _unitOfWork.CoverTypes.Remove(coverType);
            _unitOfWork.Save();
            TempData["dbmsg"] = "Cover Type Deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
