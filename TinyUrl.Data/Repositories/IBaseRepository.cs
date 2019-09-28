using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyUrl.Data.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> Query();
        void Insert(TEntity entity);
        void Insert(IEnumerable<TEntity> entities);
        void Delete(TEntity entity);
        void Delete(IEnumerable<TEntity> entities);
    }
}
