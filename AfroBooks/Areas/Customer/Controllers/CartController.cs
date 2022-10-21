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
            CalcOrderTotal();
            cartOrderViewModel.OrderHeader.OrderTotal = OrderTotal;
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
            CalcOrderTotal();

            cartOrderViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUsers.GetFirstOrDefault(u => u.Id == ApplicationUserId);
            var orderUser = cartOrderViewModel.OrderHeader.ApplicationUser;
            cartOrderViewModel.OrderHeader.PhoneNumber = orderUser.PhoneNumber;
            cartOrderViewModel.OrderHeader.StreetAddress = orderUser.StreetName;
            cartOrderViewModel.OrderHeader.City = orderUser.City;
            cartOrderViewModel.OrderHeader.Name = orderUser.Name;

            cartOrderViewModel.OrderHeader.OrderTotal = OrderTotal;
            return View(cartOrderViewModel);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPost(CartOrderViewModel cartOrderViewModel)
        {
            ApplicationUserId = GetUserId();
            cartOrderViewModel.CartProducts = _unitOfWork.CartProducts.GetAll(u => u.ApplicationUserId == ApplicationUserId, "Product");
            CalcOrderTotal(cartOrderViewModel);
            
            SetOrderHeaderProps(cartOrderViewModel.OrderHeader);
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


            // stripe settings

            var domain = HttpContext.Request.Host.Value;
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{domain}/Customer/cart/OrderConfirmation?id={cartOrderViewModel.OrderHeader.Id}",
                CancelUrl = $"{domain}/Customer/cart/Index",
            };

            foreach (var item in cartOrderViewModel.CartProducts)
                options.LineItems.Add(new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        // UnitAmount is in cents
                        UnitAmount = (long)(item.Price * 100),//20.00 -> 2000
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        },
                    },
                    Quantity = item.Count,
                });

            // create a stripe service session based on the optinos which is the cart creds
            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrdersHeaders.UpdateStripePaymentID(cartOrderViewModel.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrdersHeaders.GetFirstOrDefault(u => u.Id == id, "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //check the stripe status to see if payment is made
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrdersHeaders.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
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


        private void CalcOrderTotal()
        {
            foreach (CartProduct cart in cartOrderViewModel.CartProducts)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.PriceUnit, cart.Product.Price50Unit, cart.Product.Price100Unit);
                OrderTotal += (cart.Price * cart.Count);
            }
        }

        private void CalcOrderTotal(CartOrderViewModel cartOrderViewModel)
        {
            foreach (CartProduct cart in cartOrderViewModel.CartProducts)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.PriceUnit, cart.Product.Price50Unit, cart.Product.Price100Unit);
                OrderTotal += (cart.Price * cart.Count);
            }
        }

        private void SetOrderHeaderProps(OrderHeader workingOrderHeader)
        {
            workingOrderHeader.PaymentStatus = SD.PaymentStatusPending;
            workingOrderHeader.OrderStatus = SD.StatusPending;
            workingOrderHeader.OrderDate = DateTime.Now;
            workingOrderHeader.ApplicationUserId = ApplicationUserId;
            workingOrderHeader.OrderTotal = OrderTotal;
        }

    }
}
