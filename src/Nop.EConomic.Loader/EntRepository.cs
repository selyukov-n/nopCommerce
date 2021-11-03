using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using LinqToDB.Data;

using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;

namespace Nop.EConomic.Loader
{
    public class EntRepository<T> : IRepository<T> where T : BaseEntity
    {
        public IQueryable<T> Table { get; }

        public EntRepository(INopDataProvider dataProvider)
        {
            Table = dataProvider.GetTable<T>();
        }

        public Task DeleteAsync(T entity, bool publishEvent = true) => throw new NotImplementedException();
        public Task DeleteAsync(IList<T> entities, bool publishEvent = true) => throw new NotImplementedException();
        public Task<int> DeleteAsync(Expression<Func<T, bool>> predicate) => throw new NotImplementedException();
        public Task<IList<T>> EntityFromSqlAsync(string procedureName, params DataParameter[] parameters) => throw new NotImplementedException();
        public IList<T> GetAll(Func<IQueryable<T>, IQueryable<T>> func = null, Func<IStaticCacheManager, CacheKey> getCacheKey = null, bool includeDeleted = true) => throw new NotImplementedException();
        public Task<IList<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>> func = null, Func<IStaticCacheManager, CacheKey> getCacheKey = null, bool includeDeleted = true) => throw new NotImplementedException();
        public Task<IList<T>> GetAllAsync(Func<IQueryable<T>, Task<IQueryable<T>>> func = null, Func<IStaticCacheManager, CacheKey> getCacheKey = null, bool includeDeleted = true) => throw new NotImplementedException();
        public Task<IList<T>> GetAllAsync(Func<IQueryable<T>, Task<IQueryable<T>>> func, Func<IStaticCacheManager, Task<CacheKey>> getCacheKey, bool includeDeleted = true) => throw new NotImplementedException();
        public Task<IPagedList<T>> GetAllPagedAsync(Func<IQueryable<T>, IQueryable<T>> func = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool includeDeleted = true) => throw new NotImplementedException();
        public Task<IPagedList<T>> GetAllPagedAsync(Func<IQueryable<T>, Task<IQueryable<T>>> func = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool includeDeleted = true) => throw new NotImplementedException();
        public Task<T> GetByIdAsync(int? id, Func<IStaticCacheManager, CacheKey> getCacheKey = null, bool includeDeleted = true) => throw new NotImplementedException();
        public Task<IList<T>> GetByIdsAsync(IList<int> ids, Func<IStaticCacheManager, CacheKey> getCacheKey = null, bool includeDeleted = true) => throw new NotImplementedException();
        public Task InsertAsync(T entity, bool publishEvent = true) => throw new NotImplementedException();
        public Task InsertAsync(IList<T> entities, bool publishEvent = true) => throw new NotImplementedException();
        public Task<T> LoadOriginalCopyAsync(T entity) => throw new NotImplementedException();
        public Task TruncateAsync(bool resetIdentity = false) => throw new NotImplementedException();
        public Task UpdateAsync(T entity, bool publishEvent = true) => throw new NotImplementedException();
        public Task UpdateAsync(IList<T> entities, bool publishEvent = true) => throw new NotImplementedException();
    }
}
