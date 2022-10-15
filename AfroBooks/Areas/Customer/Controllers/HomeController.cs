using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models;
using AfroBooks.Models.ViewModels;
using AfroBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace AfroBooksWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public string ApplicationUserId { get; private set; }


        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productsList = _unitOfWork.Products.GetAll();
            return View(productsList);
        }

        [Authorize]
        public IActionResult Details(int productId)
        {
            ApplicationUserId = GetUserId();
            try
            {
                // get the previous count of the shopping cart bag that has the asame product and user. it could be zero
                int prevCount = _unitOfWork.ShoppingCartProducts.GetFirstOrDefault(u => u.ApplicationUserId == ApplicationUserId && u.ProductId == productId).Count;
                ShoppingCartProductVM shoppingCart = new()
                {
                    ProductId = productId,
                    Count = prevCount != 0 ? prevCount : 1,
                    Product = _unitOfWork.Products.GetFirstOrDefault(u => u.Id == productId, "ProductCategory", "ProductCoverType")
                };
                return View(shoppingCart);
            }
            catch (NullReferenceException)
            {
                ShoppingCartProductVM shoppingCart = new()
                {
                    ProductId = productId,
                    Count = 1,
                    Product = _unitOfWork.Products.GetFirstOrDefault(u => u.Id == productId, "ProductCategory", "ProductCoverType")
                };
                return View(shoppingCart);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCartProduct shoppingCartProduct)
        {
            ApplicationUserId = GetUserId();

            shoppingCartProduct.ApplicationUserId = ApplicationUserId;

            ShoppingCartProduct dbshoppingCartProduct = _unitOfWork.ShoppingCartProducts.GetFirstOrDefault(
            u => u.ApplicationUserId == shoppingCartProduct.ApplicationUserId && u.ProductId == shoppingCartProduct.ProductId);


            if (dbshoppingCartProduct != null)
                _unitOfWork.ShoppingCartProducts.Update(dbshoppingCartProduct, shoppingCartProduct.Count);
            else
                _unitOfWork.ShoppingCartProducts.Add(shoppingCartProduct);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        private string GetUserId()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            // claim has the Id of the user that is logged in, the extraction happes with the help of Authorize
            Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            return claim.Value;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}