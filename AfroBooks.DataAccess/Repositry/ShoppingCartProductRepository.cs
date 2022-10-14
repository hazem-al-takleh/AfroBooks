using AfroBooks.DataAccess.Data;
using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models;
using AfroBooks.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.DataAccess.Repositry
{
    public class ShoppingCartProductRepository : Repository<ShoppingCartProduct>, IShoppingCartProductRepository
    {
        private ApplicationDbContext _context;

        public ShoppingCartProductRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public void Update(ShoppingCartProduct shoppingCartProduct)
        {
            _context.ShoppingCartProducts.Update(shoppingCartProduct);
        }

        public void Update(ShoppingCartProduct shoppingCartProduct, int count)
        {
            shoppingCartProduct.Count = count;
            Update(shoppingCartProduct);
        }
    }
}
