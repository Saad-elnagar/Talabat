using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys;
using Talabat.Core.Reposatory;
using Talabat.Reposatory.Data;

namespace Talabat.Reposatory.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StoreContext _context;

        // هنخزن الـ Repositories اللي اتعملت عشان ما نعملش واحد جديد كل مرة
        private Dictionary<string, GenericRepositry<BaseEntity>> _repositories;

        public UnitOfWork(StoreContext context)
        {
            _context = context;
            _repositories = new Dictionary<string, GenericRepositry<BaseEntity>>();
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            
            var type = typeof(TEntity).Name;

            // لو الـ Repository ده اتعمل قبل كده، رجّعه من الـ Dictionary
            if (!_repositories.ContainsKey(type))
            {
                var repository = new GenericRepositry<BaseEntity>(_context);
                _repositories.Add(type, repository);
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
}
