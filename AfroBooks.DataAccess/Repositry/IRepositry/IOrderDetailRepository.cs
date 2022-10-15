using AfroBooks.Models;

namespace AfroBooks.DataAccess.Repositry.IRepositry
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        void AddRange(IEnumerable<OrderDetail> carts);
        void Update(OrderDetail obj);
    }
}