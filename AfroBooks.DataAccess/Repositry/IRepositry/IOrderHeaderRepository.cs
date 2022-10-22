using AfroBooks.Models;

namespace AfroBooks.DataAccess.Repositry.IRepositry
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripeSessionId(int id, string paymentItentId);
        void UpdateStripePaymentIntentId(int id, string paymentItentId);

    }
}

