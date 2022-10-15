using AfroBooks.DataAccess.Data;
using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models;
using AfroBooks.Models.ViewModels;
using AfroBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AfroBooksWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    [BindProperties]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public double OrderTotal { get; set; }
        public string ApplicationUserId { get; private set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            ApplicationUserId = GetUserId();

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListShoppingCartProducts = _unitOfWork.ShoppingCartProducts.GetAll(u => u.ApplicationUserId == ApplicationUserId, "Product"),
                OrderHeader = new()
            };
            foreach (var cart in ShoppingCartVM.ListShoppingCartProducts)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.PriceUnit, cart.Product.Price50Unit, cart.Product.Price100Unit);
                OrderTotal += (cart.Price * cart.Count);
            }
            ShoppingCartVM.OrderHeader.OrderTotal = OrderTotal;
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
            ApplicationUserId = GetUserId();

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListShoppingCartProducts = _unitOfWork.ShoppingCartProducts.GetAll(u => u.ApplicationUserId == ApplicationUserId, "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUsers.GetFirstOrDefault(u => u.Id == ApplicationUserId);
            var orderUser = ShoppingCartVM.OrderHeader.ApplicationUser;
            ShoppingCartVM.OrderHeader.PhoneNumber = orderUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = orderUser.StreetName;
            ShoppingCartVM.OrderHeader.City = orderUser.City;
            ShoppingCartVM.OrderHeader.Name = orderUser.Name;

            foreach (var cart in ShoppingCartVM.ListShoppingCartProducts)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.PriceUnit, cart.Product.Price50Unit, cart.Product.Price100Unit);
                OrderTotal += (cart.Price * cart.Count);
            }
            ShoppingCartVM.OrderHeader.OrderTotal = OrderTotal;
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(ShoppingCartVM ShoppingCartVM)
        {
            ApplicationUserId = GetUserId();

            ShoppingCartVM.ListShoppingCartProducts = _unitOfWork.ShoppingCartProducts.GetAll(u => u.ApplicationUserId == ApplicationUserId, "Product");
            var carts = ShoppingCartVM.ListShoppingCartProducts;

            var workingOrderHeader = ShoppingCartVM.OrderHeader;
            workingOrderHeader.PaymentStatus = SD.PaymentStatusPending;
            workingOrderHeader.OrderStatus = SD.StatusPending;
            workingOrderHeader.OrderDate = DateTime.Now;
            workingOrderHeader.ApplicationUserId = ApplicationUserId;

            foreach (var cart in carts)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.PriceUnit, cart.Product.Price50Unit, cart.Product.Price100Unit);
                OrderTotal += (cart.Price * cart.Count);
            }
            workingOrderHeader.OrderTotal = OrderTotal;

            _unitOfWork.OrdersHeaders.Add(workingOrderHeader);
            // we can't not command a save operation bc the adding of cars in db requires the id
            // of the order that MUST BE SAVED BEFORE
            _unitOfWork.Save();

            foreach (var cart in carts)
            {
                _unitOfWork.OrdersDetails.Add(new OrderDetail()
                {
                    OrderId = workingOrderHeader.Id,
                    ProductId = cart.ProductId,
                    Count = cart.Count,
                    Price = cart.Price,
                });
            }
            _unitOfWork.ShoppingCartProducts.RemoveRange(carts);
            _unitOfWork.Save();
            return RedirectToAction("Index", "Cart");
        }

        private double GetPriceBasedOnQuantity(int count, double priceUnit, double price50Unit, double price100Unit)
        {
            return count < 50 ? priceUnit : count < 100 ? price50Unit : price100Unit;
        }

        private string GetUserId()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            // claim has the Id of the user that is logged in, the extraction happes with the help of Authorize
            return claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

    }
}
