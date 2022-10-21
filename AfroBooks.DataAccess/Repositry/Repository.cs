using AfroBooks.DataAccess.Data;
using AfroBooks.DataAccess.Repositry.IRepositry;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.DataAccess.Repositry
{
    // T is the class of the table that we access to configure and use
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        // generic way of:
        // {
        // internal DbSet<Category> Categories { get; set; }
        // }
        // is
        internal DbSet<TEntity> DbSet;
        // get ApplicationDbContext using DI
        private readonly ApplicationDbContext _context;

        // create context of repositry
        // get the implemtaion of our database
        public Repository(ApplicationDbContext dbContext)
        {
            _context = dbContext;
            // in ApplicationDbContext all transaciton are done on a dbset
            // we must get an instance of dbset and work on it directly

            // get the DbSet of T through Set<Type>()

            // we point the repositry DbSet to whatever table/DbSet the database may lead to
            // in the context that the DbSet is the container of all entities
            this.DbSet = _context.Set<TEntity>();
        }

        //public Repository(DbContextOptions<ApplicationDbContext> dbContext)
        //{

        //}

        public void Add(TEntity entity)
        {
            DbSet.Add(entity);
        }

        public IEnumerable<TEntity> GetAll()
        {
            // query data; all entities in the DbSet to a query
            IQueryable<TEntity> query = DbSet;
            return query.ToList();
        }

        public TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>> filter)
        {
            IQueryable<TEntity>? query = DbSet;
            query = query.Where(filter);
            return query.FirstOrDefault();
        }

        public void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entity)
        {
            DbSet.RemoveRange(entity);
        }

        public IEnumerable<TEntity> GetAll(params string[] includedProperties)
        {
            // query data; all entities in the DbSet to a query
            IQueryable<TEntity> query = DbSet;
            query = _IncludeProperties(includedProperties, query);
            return query.ToList();
        }

        public TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>> filter, params string[] includedProperties)
        {
            IQueryable<TEntity>? query = DbSet;
            query = query.Where(filter);
            query = _IncludeProperties(includedProperties, query);
            return query.FirstOrDefault();
        }

        public IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> filter, params string[] includedProperties)
        {
            IQueryable<TEntity> query = DbSet;
            query = query.Where(filter);
            query = _IncludeProperties(includedProperties, query);
            return query.ToList();
        }

        private static IQueryable<TEntity> _IncludeProperties(string[] includedProperties, IQueryable<TEntity> query)
        {
            foreach (string property in includedProperties)
                query = query.Include(property);
            return query;
        }

        public TEntity? GetFirstOrDefaultNullable(Expression<Func<TEntity, bool>> filter)
        {
            IQueryable<TEntity>? query = DbSet;
            query = query.Where(filter);
            return query.FirstOrDefault();
        }
    }
}
