using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys;

namespace Talabat.Core.Reposatory
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        // دي بتجيبلك أي Repository Generic بالنوع اللي تحبه
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;

        // دي اللي بتعمل SaveChanges
        Task<int> CompleteAsync();
    }
}
