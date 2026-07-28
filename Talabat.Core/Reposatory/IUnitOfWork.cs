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
        
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;

        
        Task<int> CompleteAsync();
    }
}
