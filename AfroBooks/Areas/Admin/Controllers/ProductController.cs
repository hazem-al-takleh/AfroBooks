using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models;
using AfroBooks.Models.ViewModels;
using AfroBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AfroBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [BindProperties]
    [Authorize(Roles = SD.RoleAdmin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET
        // id passed to controller from Index view
        public IActionResult Upsert(int? id)
        {
            // Instead of ViewBag
            ProductViewModel productViewModel = new()
            {
                Product = new(),
                CategoriesList = _unitOfWork.Categories.GetAll().Select(u => new SelectListItem(text: u.Name, value: u.ID.ToString())).ToList(),
                CoverTypesList = _unitOfWork.CoverTypes.GetAll().Select(u => new SelectListItem(text: u.Name, value: u.Id.ToString())).ToList()
            };

            // create product
            if (id is null || id == 0)
                return View(productViewModel);

            // else update product
            productViewModel.Product = _unitOfWork.Products.GetFirstOrDefault(u => u.Id == id);
            return View(productViewModel);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel productViewModel, IFormFile? formFile)
        {
            Product productVM = productViewModel.Product;
            int? id = productVM.Id;
            if (!ModelState.IsValid)
                return View(new ProductViewModel
                {
                    Product = _unitOfWork.Products.GetFirstOrDefault(u => u.Id == id),
                    CategoriesList = _unitOfWork.Categories.GetAll().Select(u => new SelectListItem(text: u.Name, value: u.ID.ToString())).ToList(),
                    CoverTypesList = _unitOfWork.CoverTypes.GetAll().Select(u => new SelectListItem(text: u.Name, value: u.Id.ToString())).ToList()
                });
            // delete product img if new one uploaded and old one exists
            if (formFile != null && id != 0)
                _DeleteOldImgFile(productVM);
            // upload a new product img if formFile uploaded
            if (formFile != null)
                _UploadImage(productVM, formFile);

            // update product
            if (id != 0)
            {
                _unitOfWork.Products.Update(productVM);
                TempData["dbmsg"] = "Product Updated successfully";
            }
            // create product
            else
            {
                _unitOfWork.Products.Add(productVM);
                TempData["dbmsg"] = "Product Created successfully";
            }
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        #region Helper Methods
        /// <summary>
        /// Deletes the img file associated with the product object
        /// </summary>
        /// <param name="product"></param>
        private void _DeleteOldImgFile(Product product)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string oldRelariveImgPath = product.CoverImageURL;
            string oldAbsoluteImgPath = Path.Combine(wwwRootPath, oldRelariveImgPath);
            if (System.IO.File.Exists(oldAbsoluteImgPath))
                System.IO.File.Delete(oldAbsoluteImgPath);
        }
        /// <summary>
        /// Uploads the img file formFile to physical drive with a unique id
        /// and updates product object with the unique id file path/URL
        /// </summary>
        /// <param name="product"></param>
        /// <param name="formFile"></param>
        private void _UploadImage(Product product, IFormFile formFile)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string absoluteUploadFolder = Path.Combine(wwwRootPath, "images\\products");
            string relativeUploadFolder = "images\\products";

            string fileName = Guid.NewGuid().ToString();
            string extension = Path.GetExtension(formFile.FileName);

            using (var fileStream = new FileStream(Path.Combine(absoluteUploadFolder, fileName + extension), FileMode.Create))
                formFile.CopyTo(fileStream);


            product.CoverImageURL = Path.Combine(relativeUploadFolder, fileName + extension);
        }

        #endregion

        #region API Calls
        //[Route("/api/v1/Product/GetAll")]
        [HttpGet]
        public IActionResult GetAll()
        {
            IEnumerable<Product> productsList = _unitOfWork.Products.GetAll("ProductCategory", "ProductCoverType");
            return Json(new { data = productsList });
        }

        public IActionResult Delete(int? id)
        {
            if (id is null)
                return Json(new { state = false, msg = "Error While Deleting Product" });
            var product = _unitOfWork.Products.GetFirstOrDefault(u => u.Id == id);
            if (product is null)
                return Json(new { state = false, msg = "Error While Deleting Product" });

            _DeleteOldImgFile(product);
            _unitOfWork.Products.Remove(product);
            _unitOfWork.Save();
            return Json(new { state = true, msg = "Product Deleted Successfully" });
        }

        #endregion

    }
}
