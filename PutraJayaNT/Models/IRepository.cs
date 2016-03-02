using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PutraJayaNT.Models
{
    interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate = null);
        T Get(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        long Count();
    }
}
