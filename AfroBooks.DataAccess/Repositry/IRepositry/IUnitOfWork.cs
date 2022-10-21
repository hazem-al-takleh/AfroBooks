using AfroBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.DataAccess.Repositry.IRepositry
{
    public interface IUnitOfWork
    {
        ICategoryRepository Categories { get; }
        ICoverTypeRepository CoverTypes { get; }
        IProducrRepository Products { get; }
        ICompanyRepository Companies { get; }
        IApplicationUserRepository ApplicationUsers { get; }
        ICartProductRepository CartProducts { get; }

        IOrderDetailRepository OrdersDetails { get; }
        IOrderHeaderRepository OrdersHeaders { get; }



        void Save();
    }
}
