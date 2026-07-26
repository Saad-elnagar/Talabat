using System.Collections;
using Talabat.Core.Entitys;
using Talabat.Core.Reposatory;
using Talabat.Reposatory.Data;
using Talabat.Reposatory.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly StoreContext _context;
    private Hashtable _repositories;

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
            //
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