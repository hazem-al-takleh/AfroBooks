using AfroBooks.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AfroBooks.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        // We will recieve some options that are going to be passed to the base class DbContext
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {   

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<CoverType> CoverTypes { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CartProduct> CartProducts { get; set; }
        public DbSet<OrderHeader> OrdersHeaders { get; set; }
        public DbSet<OrderDetail> OrdersDetails { get; set; }
    }
}
