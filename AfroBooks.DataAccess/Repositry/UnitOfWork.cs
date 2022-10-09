using AfroBooks.DataAccess.Data;
using AfroBooks.DataAccess.Repositry.BulkyBook.DataAccess.Repository;
using AfroBooks.DataAccess.Repositry.IRepositry;
using AfroBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.DataAccess.Repositry
{
    public class UnitOfWork : IUnitOfWork
    {
        //Caetgory repo context
        public ICategoryRepository Categories { get; private set; }
        public ICoverTypeRepository CoverTypes { get; private set; }
        public IProducrRepository Products { get; private set; }
        public ICompanyRepository Companies { get; private set; }

        private ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _context = dbContext;
            Categories = new CategoryRepository(_context);
            CoverTypes = new CoverTypeRepository(_context);
            Products = new ProductRepository(_context);
            Companies = new CompanyRepository(_context);
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
