using Microsoft.EntityFrameworkCore.Storage;
using Taskify.Domain.Entities;
using System.Linq.Expressions;



namespace Taskify.DataStore.Repositorise.Interface
{
    public interface IAppRepository<T> where T : BaseEntity
    {
        Task<T> GetByIdAsync(Guid id);
        IQueryable<T> GetAllAsync(bool trackChanges = false);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        IQueryable<T> FindByCondition(Expression<Func<T,bool>> prediction, bool trackChanges = false);
        Task<bool> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
