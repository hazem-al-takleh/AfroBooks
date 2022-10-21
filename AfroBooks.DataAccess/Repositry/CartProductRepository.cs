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
    public class CartProductRepository : Repository<CartProduct>, ICartProductRepository
    {
        private ApplicationDbContext _context;

        public CartProductRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public void Update(CartProduct cartProduct)
        {
            _context.CartProducts.Update(cartProduct);
        }

        public void Update(CartProduct cartProduct, int count)
        {
            cartProduct.Count = count;
            Update(cartProduct);
        }
    }
}
