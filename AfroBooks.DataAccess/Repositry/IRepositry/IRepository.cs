using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AfroBooks.DataAccess.Repositry.IRepositry
{
    public interface IRepository<TEntity> where TEntity : class
    { 
        IEnumerable<TEntity> GetAll();
        TEntity? GetFirstOrDefaultNullable(Expression<Func<TEntity, bool>> filter);
        TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>> filter);
        IEnumerable<TEntity> GetAll(params string[] includedProperties);
        TEntity GetFirstOrDefault(Expression<Func<TEntity, bool>> filter, params string[] includedProperties);
        void Add(TEntity entity);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entity);
        // GetAll + GetFirstOrDefault = GetAllFiltered
        IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> filter, params string[] includedProperties);


        // not included because its logic is not generic in all tables/objects
        //void Update(T entity);
    }
}
