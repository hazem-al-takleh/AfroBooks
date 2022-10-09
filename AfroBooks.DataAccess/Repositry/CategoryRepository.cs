using AfroBooks.DataAccess.Data;
using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.DataAccess.Repositry
{
    namespace BulkyBook.DataAccess.Repository
    {
        public class CategoryRepository : Repository<Category>, ICategoryRepository
        {
            private ApplicationDbContext _context;

            public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext)
            {
                _context = dbContext;
            }

            public void Update(Category obj)
            {
                _context.Categories.Update(obj);
            }
        }
    }
}
