using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models;
using AfroBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AfroBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;


        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Company> companies = _unitOfWork.Companies.GetAll();
            return View(companies);
        }

        // GET
        public IActionResult Create()
        {
            return View();
        }

        // POST
        [HttpPost]
        // validate the token on all requests
        [ValidateAntiForgeryToken]
        //overrided version of create action that takes the category obj from user and validates it
        public IActionResult Create(Company company)
        {
            if (!ModelState.IsValid)
                return View(company);

            //_unitOfWork.Categories.Add(obj);    
            //_unitOfWork.SaveChanges();
            _unitOfWork.Companies.Add(company);
            _unitOfWork.Save();
            TempData["dbmsg"] = "Company Created successfully";
            return RedirectToAction("Index");

        }

        // GET
        public IActionResult Update(int? id)
        {
            if (id is null)
                return NotFound();
            var company = _unitOfWork.Companies.GetFirstOrDefault(u => u.Id == id);
            if (company is null)
                return NotFound();
            return View(company);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Company company)
        {
            if (!ModelState.IsValid)
                return View(company);

            _unitOfWork.Companies.Update(company);
            _unitOfWork.Save();
            TempData["dbmsg"] = "Company Updated successfully";
            return RedirectToAction("Index");

        }

        // GET
        public IActionResult Delete(int? id)
        {
            if (id is null)
                return NotFound();
            var company = _unitOfWork.Companies.GetFirstOrDefault(u => u.Id == id);
            if (company is null)
                return NotFound();
            return View(company);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Company company)
        {
            _unitOfWork.Companies.Remove(company);
            _unitOfWork.Save();
            TempData["dbmsg"] = "Company Deleted successfully";
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            Company company = _unitOfWork.Companies.GetFirstOrDefault(u => u.Id == id);
            return View(company);
        }

    }
}
