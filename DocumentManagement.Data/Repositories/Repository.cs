using DocumentManagement.Data.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DocumentManagement.Data.Repositories
{
    public abstract class Repository<TEntity, TContext> : IRepository<TEntity> where TEntity : class, IEntity where TContext : DocumentManagementContext

    {

        private readonly TContext _context;

        public Repository(TContext context)
        {
            this._context = context;
        }

        public virtual async Task<TEntity> Add(TEntity entity)
        {
            try
            {
                await _context.Set<TEntity>().AddAsync(entity);
                return entity;
            }
            catch (Exception x)
            {
                _context.Rollback();
                throw (x);
            }
        }

        public virtual async Task<TEntity> Delete(int id)
        {
            var entity = await _context.Set<TEntity>().FindAsync(id);
            if (entity == null)
            {
                return entity;
            }

            _context.Set<TEntity>().Remove(entity);

            return entity;
        }

        public virtual async Task ReloadAsync(TEntity entity)
        {
            if (entity != null)
            {
                await _context.Entry(entity).ReloadAsync();
            }
        }

        public virtual async Task<TEntity> Get(int id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public virtual async Task<List<TEntity>> GetAll()
        {
            return await EntityFrameworkQueryableExtensions.ToListAsync(_context.Set<TEntity>());
        }

        public virtual async Task<int> CountAll()
        {
            return await EntityFrameworkQueryableExtensions.CountAsync(_context.Set<TEntity>());
        }

        public virtual async void AddRange(List<TEntity> entities)
        {
            try
            {
                await _context.Set<TEntity>().AddRangeAsync(entities);
            }
            catch (Exception x)
            {
                _context.Rollback();
                throw (x);
            }
        }

        /// <summary>
        /// Update the T object  In case AsNoTracking is used
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>async T object</returns>
        public virtual async Task<TEntity> Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return await Task.FromResult(entity);
        }

        public abstract Task<InfoDto> Validate(TEntity entity);

        public void DeleteRange(List<TEntity> entities)
        {
            try
            {
                _context.Set<TEntity>().RemoveRange(entities);
            }
            catch (Exception x)
            {
                _context.Rollback();
                throw (x);
            }
        }
    }

}
