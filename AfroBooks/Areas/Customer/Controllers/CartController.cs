using AfroBooks.DataAccess.Data;
using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models;
using AfroBooks.Models.ViewModels;
using AfroBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using System.Linq;
using Stripe.Issuing;

namespace AfroBooksWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    [BindProperties]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartOrderViewModel cartOrderViewModel { get; set; }
        public double OrderTotal { get; set; }
        public string ApplicationUserId { get; private set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            ApplicationUserId = GetUserId();

            cartOrderViewModel = new CartOrderViewModel()
            {
                CartProducts = _unitOfWork.CartProducts.GetAll(u => u.ApplicationUserId == ApplicationUserId, "Product"),
                OrderHeader = new()
            };
            cartOrderViewModel.OrderHeader.OrderTotal = CalcOrderTotal();
            return View(cartOrderViewModel);
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.CartProducts.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.CartProducts.Update(cart, cart.Count + 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.CartProducts.GetFirstOrDefault(u => u.Id == cartId);
            if (cart.Count > 1)
                _unitOfWork.CartProducts.Update(cart, cart.Count - 1);
            else
                _unitOfWork.CartProducts.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.CartProducts.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.CartProducts.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            ApplicationUserId = GetUserId();
            cartOrderViewModel = new CartOrderViewModel()
            {
                CartProducts = _unitOfWork.CartProducts.GetAll(u => u.ApplicationUserId == ApplicationUserId, "Product"),
                OrderHeader = new()
            };

            cartOrderViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUsers.GetFirstOrDefault(u => u.Id == ApplicationUserId);
            var orderUser = cartOrderViewModel.OrderHeader.ApplicationUser;
            cartOrderViewModel.OrderHeader.PhoneNumber = orderUser.PhoneNumber;
            cartOrderViewModel.OrderHeader.StreetAddress = orderUser.StreetName;
            cartOrderViewModel.OrderHeader.City = orderUser.City;
            cartOrderViewModel.OrderHeader.Name = orderUser.Name;

            cartOrderViewModel.OrderHeader.OrderTotal = CalcOrderTotal();
            return View(cartOrderViewModel);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(CartOrderViewModel cartOrderViewModel)
        {
            ApplicationUserId = GetUserId();
            cartOrderViewModel.CartProducts = _unitOfWork.CartProducts.GetAll(u => u.ApplicationUserId == ApplicationUserId, "Product");
            OrderTotal = CalcOrderTotal(cartOrderViewModel);


            cartOrderViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            cartOrderViewModel.OrderHeader.OrderStatus = SD.StatusPending;
            cartOrderViewModel.OrderHeader.OrderDate = DateTime.Now;
            cartOrderViewModel.OrderHeader.ApplicationUserId = ApplicationUserId;
            cartOrderViewModel.OrderHeader.OrderTotal = OrderTotal;

            int? companyId = _unitOfWork
                .ApplicationUsers
                .GetFirstOrDefault(u => u.Id == ApplicationUserId)
                .CompanyId;



            if (companyId != null)
            {
                cartOrderViewModel.OrderHeader.OrderStatus = SD.StatusApproved;
                cartOrderViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                cartOrderViewModel.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                _unitOfWork.OrdersHeaders.Add(cartOrderViewModel.OrderHeader);
                _unitOfWork.Save();
                List<OrderDetail> orderDetails = (from cart in cartOrderViewModel.CartProducts
                                                  select new OrderDetail()
                                                  {
                                                      OrderId = cartOrderViewModel.OrderHeader.Id,
                                                      ProductId = cart.ProductId,
                                                      Count = cart.Count,
                                                      Price = cart.Price,
                                                  }).ToList();
                _unitOfWork.OrdersDetails.AddRange(orderDetails);
                _unitOfWork.Save();

                return RedirectToAction("OrderConfirmation", "Cart", new { id = cartOrderViewModel.OrderHeader.Id });
            }

            cartOrderViewModel.OrderHeader.OrderStatus = SD.StatusPending;
            cartOrderViewModel.OrderHeader.PaymentStatus = SD.PaymentStatusPending;


            _unitOfWork.OrdersHeaders.Add(cartOrderViewModel.OrderHeader);
            // we can't not command a save operation bc the adding of cars in db requires the id
            // of the order that MUST BE SAVED BEFORE (depend on it)
            _unitOfWork.Save();

            List<OrderDetail> cartToDb = (from cart in cartOrderViewModel.CartProducts
                                          select new OrderDetail()
                                          {
                                              OrderId = cartOrderViewModel.OrderHeader.Id,
                                              ProductId = cart.ProductId,
                                              Count = cart.Count,
                                              Price = cart.Price,
                                          }).ToList();
            _unitOfWork.OrdersDetails.AddRange(cartToDb);

            // stripe optinos for Indivisual users
            string domain = "https://localhost:7205";
            var options = new SessionCreateOptions()
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = (from cart in cartOrderViewModel.CartProducts
                             select new SessionLineItemOptions()
                             {
                                 PriceData = new SessionLineItemPriceDataOptions()
                                 {
                                     UnitAmount = (long)(cart.Price * 100),
                                     Currency = "usd",
                                     ProductData = new SessionLineItemPriceDataProductDataOptions
                                     {
                                         Name = cart.Product.Title,
                                     },
                                 },
                                 Quantity = cart.Count
                             }).ToList(),
                Mode = "payment",
                SuccessUrl = domain + $"/Customer/cart/OrderConfirmation?id={cartOrderViewModel.OrderHeader.Id}",
                CancelUrl = domain + $"/Customer/cart/Index",
            };

            // create a stripe service session based on the optinos which is the cart creds
            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrdersHeaders.UpdateStripeSessionId(cartOrderViewModel.OrderHeader.Id, session.Id);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrdersHeaders.GetFirstOrDefault(u => u.Id == id, "ApplicationUser");

            // check if user is not company and has not payed yet
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                string paymentIntentId = service.Get(orderHeader.SessionId).PaymentIntentId;
                _unitOfWork.OrdersHeaders.UpdateStripePaymentIntentId(id, paymentIntentId);
                //check the stripe status to see if payment is made
                if (session.PaymentStatus.ToLower() == "paid")
                    _unitOfWork.OrdersHeaders.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                _unitOfWork.Save();
            }
            //_emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book", "<p>New Order Created</p>");
            List<CartProduct> shoppingCarts = _unitOfWork
                .CartProducts
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId)
                .ToList();
            _unitOfWork.CartProducts.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
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


        private double CalcOrderTotal()
        {
            double OrderTotal = 0;
            foreach (CartProduct cart in cartOrderViewModel.CartProducts)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.PriceUnit, cart.Product.Price50Unit, cart.Product.Price100Unit);
                OrderTotal += (cart.Price * cart.Count);
            }
            return OrderTotal;
        }

        private double CalcOrderTotal(CartOrderViewModel cartOrderViewModel)
        {
            double OrderTotal = 0;
            foreach (CartProduct cart in cartOrderViewModel.CartProducts)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.PriceUnit, cart.Product.Price50Unit, cart.Product.Price100Unit);
                OrderTotal += (cart.Price * cart.Count);
            }
            return OrderTotal;
        }

    }
}
