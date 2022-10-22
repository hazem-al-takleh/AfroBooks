using AfroBooks.DataAccess.Data;
using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.DataAccess.Repositry
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _context;

        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }


        public void Update(OrderHeader obj)
        {
            _context.OrdersHeaders.Update(obj);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _context.OrdersHeaders.FirstOrDefault(u => u.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (paymentStatus != null)
                    orderFromDb.PaymentStatus = paymentStatus;
            }
        }

        public void UpdateStripeSessionId(int id, string sessionId)
        {
            var orderFromDb = _context.OrdersHeaders.FirstOrDefault(u => u.Id == id);
            orderFromDb.SessionId = sessionId;
        }

        public void UpdateStripePaymentIntentId(int id, string paymentItentId)
        {
            var orderFromDb = _context.OrdersHeaders.FirstOrDefault(u => u.Id == id);
            orderFromDb.PaymentIntentId = paymentItentId;
            orderFromDb.PaymentDate = DateTime.Now;
        }

    }
}
