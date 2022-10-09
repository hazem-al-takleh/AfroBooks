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
    public class ProductRepository : Repository<Product>, IProducrRepository
    {
        ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {

            Product entity = _context.Products.FirstOrDefault(u => u.Id == product.Id);
            if (entity is null)
                return;
            // this approach preserves properties from updates that are not edited
            entity.Title = product.Title;
            entity.Description = product.Description;
            entity.ISBN =product.ISBN;
            entity.Author = product.Author;
            entity.ListPrice = product.ListPrice;
            entity.PriceUnit = product.PriceUnit;
            entity.Price50Unit = product.Price50Unit;
            entity.Price100Unit = product.Price100Unit;
            entity.CategoryId = product.CategoryId;
            entity.CoverTypeId = product.CoverTypeId;
            // 
            // if passed imgurl is populated we will update it
            if (product.CoverImageURL != null)
                entity.CoverImageURL = product.CoverImageURL;

        }
    }
}
