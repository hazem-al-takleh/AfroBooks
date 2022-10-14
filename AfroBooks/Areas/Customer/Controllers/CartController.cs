using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models.ViewModels;
using AfroBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AfroBooksWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public double OrderTotal { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListShoppingCartProducts = _unitOfWork.ShoppingCartProducts.GetAll(u => u.ApplicationUserId == GetUserId(), "Product")
            };
            foreach (var cart in ShoppingCartVM.ListShoppingCartProducts)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.PriceUnit, cart.Product.Price50Unit, cart.Product.Price100Unit);
                OrderTotal += (cart.Price * cart.Count);
            }
            ShoppingCartVM.OrderTotal = OrderTotal;
            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCartProducts.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCartProducts.Update(cart, cart.Count + 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCartProducts.GetFirstOrDefault(u => u.Id == cartId);
            if (cart.Count > 1)
                _unitOfWork.ShoppingCartProducts.Update(cart, cart.Count - 1);
            else
                _unitOfWork.ShoppingCartProducts.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCartProducts.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCartProducts.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            return View();
        }


        private double GetPriceBasedOnQuantity(int count, double priceUnit, double price50Unit, double price100Unit)
        {
            return count < 50 ? priceUnit : count < 100 ? price50Unit : price100Unit;
        }

        private string GetUserId()
        {
            return GetUserClaim().Value;
        }

        private Claim GetUserClaim()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            // claim has the Id of the user that is logged in, the extraction happes with the help of Authorize
            Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            return claim;
        }
    }
}
