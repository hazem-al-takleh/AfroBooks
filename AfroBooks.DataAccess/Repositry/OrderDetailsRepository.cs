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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDbContext _context;

        public OrderDetailRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void AddRange(IEnumerable<OrderDetail> carts)
        {
            _context.OrdersDetails.AddRange(carts);
        }

        public void Update(OrderDetail obj)
        {
            _context.OrdersDetails.Update(obj);
        }
    }
}
