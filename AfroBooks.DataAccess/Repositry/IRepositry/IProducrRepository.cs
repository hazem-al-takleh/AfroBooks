using AfroBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.DataAccess.Repositry.IRepositry
{
    public interface IProducrRepository : IRepository<Product>
    {

        void Update(Product product);
    }
}
