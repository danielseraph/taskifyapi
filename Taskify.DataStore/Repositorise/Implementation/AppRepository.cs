using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using Taskify.DataStore.Repositorise.Interface;
using Taskify.Domain.Entities;
using Taskify.Infrastructure.Persistence;

namespace Taskify.DataStore.Repositorise.Implementation
{
    public class AppRepository<T> : IAppRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext _dbContext;
        public AppRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(T entity)
        {
            entity.CreateAT = DateTime.UtcNow;
            await _dbContext.Set<T>().AddAsync(entity);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        //public  Task DeleteAsync(T entity)
        //{
        //    _dbContext.Remove(entity);
        //    return Task.CompletedTask;
        //}
        public Task DeleteAsync(T entity)
        {
            // Check if entity has "IsDeleted" property
            var property = entity.GetType().GetProperty("IsDeleted");
            if (property != null && property.PropertyType == typeof(bool))
            {
                // Set IsDeleted = true instead of removing
                property.SetValue(entity, true);
                _dbContext.Update(entity);
            }
            else
            {
                // Fall back to hard delete if no IsDeleted property
                _dbContext.Remove(entity);
            }

            return Task.CompletedTask;
        }

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> predicate, bool trackChanges = false)
        {
            var set = trackChanges ? _dbContext.Set<T>() : _dbContext.Set<T>().AsNoTracking();
            return set.Where(predicate);
        }

        public IQueryable<T> GetAllAsync(bool trackChanges = false)
        {
            return  trackChanges ? _dbContext.Set<T>() : _dbContext.Set<T>().AsNoTracking();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            var set = _dbContext.Set<T>();
            return await set.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> SaveChangesAsync()
        {
            var saveCount = await _dbContext.SaveChangesAsync();
            return saveCount > 0;
        }

        public  Task UpdateAsync(T entity)
        {
            entity.UpdateAT = DateTime.UtcNow;
            _dbContext.Set<T>().Update(entity);
            return Task.CompletedTask;
        }

    }
}
