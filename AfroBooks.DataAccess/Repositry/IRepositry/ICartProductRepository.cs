using AfroBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.DataAccess.Repositry.IRepositry
{
    public interface ICartProductRepository : IRepository<CartProduct>
    {
        void Update(CartProduct shoppingCartProduct);
        void Update(CartProduct shoppingCartProduct, int count);

    }
}
