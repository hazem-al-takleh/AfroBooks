using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models;
using AfroBooks.Models.ViewModels;
using AfroBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace AfroBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderManagementViewModel OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new OrderManagementViewModel()
            {
                OrderHeader = _unitOfWork.OrdersHeaders.GetFirstOrDefault(u => u.Id == orderId, "ApplicationUser"),
                OrderDetail = _unitOfWork.OrdersDetails.GetAll(u => u.OrderId == orderId, "Product"),
            };
            return View(OrderVM);
        }

        [ActionName("DetailsPayNow")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DetailsPayNow(int orderId)
        {
            OrderVM = new OrderManagementViewModel()
            {
                OrderHeader = _unitOfWork.OrdersHeaders.GetFirstOrDefault(u => u.Id == orderId, "ApplicationUser"),
                OrderDetail = _unitOfWork.OrdersDetails.GetAll(u => u.OrderId == orderId, "Product"),
            };


            // stripe optinos for Indivisual users
            string domain = "https://localhost:7205";
            var options = new SessionCreateOptions()
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = (from cart in OrderVM.OrderDetail
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
                SuccessUrl = domain + $"/Admin/Order/PaymentConfirmation?id={orderId}",
                CancelUrl = domain + $"/Admin/Order/Index",
            };

            // create a stripe service session based on the optinos which is the cart creds
            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrdersHeaders.UpdateStripeSessionId(orderId, session.Id);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrdersHeaders.GetFirstOrDefault(u => u.Id == id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                _unitOfWork.OrdersHeaders.UpdateStripePaymentIntentId(id, session.PaymentIntentId);

                //check the stripe status
                if (session.PaymentStatus.ToLower() == "paid")
                    _unitOfWork.OrdersHeaders.UpdateStatus(id, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                _unitOfWork.Save();
            }
            return View(id);
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail()
        {
            var orderHEaderFromDb = _unitOfWork.OrdersHeaders.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHEaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHEaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHEaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHEaderFromDb.City = OrderVM.OrderHeader.City;
            if (OrderVM.OrderHeader.Carrier != null)
                orderHEaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            if (OrderVM.OrderHeader.TrackingNumber != null)
                orderHEaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            _unitOfWork.OrdersHeaders.Update(orderHEaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = orderHEaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrdersHeaders.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Status Updated Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrdersHeaders.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            //if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            //    orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            _unitOfWork.OrdersHeaders.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrdersHeaders.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {

                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrdersHeaders.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrdersHeaders.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();

            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction("Details", "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.RoleAdmin) || User.IsInRole(SD.RoleEmployee))
            {
                orderHeaders = _unitOfWork.OrdersHeaders.GetAll("ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrdersHeaders.GetAll(u => u.ApplicationUserId == claim.Value, "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }


            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
