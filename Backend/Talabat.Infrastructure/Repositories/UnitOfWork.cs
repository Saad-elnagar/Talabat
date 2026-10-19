using System.Collections;
using Talabat.Domain.Entities;
using Talabat.Domain.Repositories;
using Talabat.Infrastructure.Persistence;
using Talabat.Infrastructure.Repositories;

namespace Talabat.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly StoreContext _context;
    private Hashtable _repositories;  // <-- object مش GenericRepository<BaseEntity>

    public UnitOfWork(StoreContext context)
    {
        _context = context;
        _repositories = new Hashtable();
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity).Name;

        if (!_repositories.ContainsKey(type))
        {
            // ✅ اعمل instance من GenericRepository<TEntity> مش <BaseEntity>
            var repositoryType = typeof(GenericRepositry<>);
            var repositoryInstance = Activator.CreateInstance(
                repositoryType.MakeGenericType(typeof(TEntity)),
                _context
            );

            _repositories.Add(type, repositoryInstance);
        }

        return (IGenericRepository<TEntity>)_repositories[type];
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}