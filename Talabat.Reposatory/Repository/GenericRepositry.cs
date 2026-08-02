using Microsoft.EntityFrameworkCore;
using Talabat.Core.Entitys;
using Talabat.Core.Reposatory;
using Talabat.Reposatory.Data;
using Talabat.Reposatory.Specification;

namespace Talabat.Reposatory.Repository
{
    public class GenericRepositry<T> : IGenericRepository<T> where T : BaseEntity
    {

        private readonly StoreContext _context;

        public GenericRepositry(StoreContext context)
        {
            _context = context;
        }
        public async Task<IReadOnlyList<T>> GetAllAsync()
            => await _context.Set<T>().ToListAsync();

        public async Task<T> GetByIdAsync(int id)

        => await _context.Set<T>().FindAsync(id);


        public async Task<T> GetByIdWithSpecAsync(ISpecification<T> spec)
            => await ApplySpecification(spec).FirstOrDefaultAsync();



        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec)
            => await ApplySpecification(spec).ToListAsync();









        public async Task<int> GetCountAsync(ISpecification<T> spec)
          => await ApplySpecification(spec).CountAsync();


        public async Task Add(T entity)

          => await _context.Set<T>().AddAsync(entity);


        public void Update(T entity)

       => _context.Set<T>().Update(entity);



        public void Delete(T entity)

        => _context.Set<T>().Remove(entity);

        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)


           => await ApplySpecification(spec).ToListAsync();



        public IQueryable<T> ApplySpecification(ISpecification<T> spec)

           => Evalutore<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);

    }

}