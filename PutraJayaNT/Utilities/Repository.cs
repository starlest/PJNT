using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace PutraJayaNT.Utilities
{
    class Repository<T> : IRepository<T> where T : class
    {
        private ERPContext m_Context = null;
        DbSet<T> m_DbSet;

        public Repository(ERPContext context)
        {
            m_Context = context;
            m_DbSet = m_Context.Set<T>();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate != null) return m_DbSet.Where(predicate);
            return m_DbSet.AsEnumerable();
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            return m_DbSet.FirstOrDefault(predicate);
        }

        public void Add(T entity)
        {
            m_DbSet.Add(entity);
        }

        public void Update(T entity)
        {
            m_DbSet.Attach(entity);
            ((IObjectContextAdapter)m_Context).ObjectContext.
            ObjectStateManager.ChangeObjectState(entity, EntityState.Modified);
        }

        public void Delete(T entity)
        {
            m_DbSet.Remove(entity);
        }

        public long Count()
        {
            return m_DbSet.Count();
        }
    }
}
