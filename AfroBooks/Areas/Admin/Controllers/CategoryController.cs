using AfroBooks.DataAccess.Data;
using AfroBooks.DataAccess.Repositry;
using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models;
using AfroBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AfroBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.RoleAdmin)]
    public class CategoryController : Controller
    {
        // repo pattern subsitute the  ApplicationDbContext with a Category repo
        // // // private readonly ApplicationDbContext _unitOfWork;
        private readonly IUnitOfWork _unitOfWork;

        // db object in the arg have all the data required to connect this controller to the database
        // through the AddDbContext Service in Program.cs
        // as we access the controller we open a connection between the controller and the database
        // via object assignmet of the controller instance property and the dbContext service

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> categoriesList = _unitOfWork.Categories.GetAll();
            return View(categoriesList);
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
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            //_unitOfWork.Categories.Add(obj);    
            //_unitOfWork.SaveChanges();
            _unitOfWork.Categories.Add(category);
            _unitOfWork.Save();
            TempData["dbmsg"] = "Category Created successfully";
            return RedirectToAction("Index");

        }

        // GET
        public IActionResult Update(int? id)
        {
            if (id is null)
                return NotFound();
            var category = _unitOfWork.Categories.GetFirstOrDefault(u => u.ID == id);
            if (category is null)
                return NotFound();
            return View(category);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            _unitOfWork.Categories.Update(category);
            _unitOfWork.Save();
            TempData["dbmsg"] = "Category Updated successfully";
            return RedirectToAction("Index");

        }

        // GET
        public IActionResult Delete(int? id)
        {
            if (id is null)
                return NotFound();
            var category = _unitOfWork.Categories.GetFirstOrDefault(u => u.ID == id);
            if (category is null)
                return NotFound();
            return View(category);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Category category)
        {
            _unitOfWork.Categories.Remove(category);
            _unitOfWork.Save();
            TempData["dbmsg"] = "Category Deleted successfully";
            return RedirectToAction("Index");
        }

        public IActionResult Details()
        {
            return View();
        }

    }
}
