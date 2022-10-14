using AfroBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.DataAccess.Repositry.IRepositry
{
    public interface IShoppingCartProductRepository : IRepository<ShoppingCartProduct>
    {
        void Update(ShoppingCartProduct shoppingCartProduct);
        void Update(ShoppingCartProduct shoppingCartProduct, int count);

    }
}
