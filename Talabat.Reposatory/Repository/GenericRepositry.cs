using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys;
using Talabat.Core.Reposatory;
using Talabat.Reposatory.Data.Config;
using Talabat.Reposatory.Specification;

namespace Talabat.Reposatory.Repository
{
    public class GenericRepositry<T> : IGenericRepository<T> where T : Base
    {

        private readonly StoreContext _context;

        public GenericRepositry(StoreContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _context.Set<T>().ToListAsync();
            }
            catch (Exception ex)
            {
                // Handle the exception as needed (e.g., log it)
                throw new Exception("An error occurred while retrieving data.", ex);
            }

        }

        public async Task<T> GetByIdAsync(int id)
        {
            try
            {

                return await _context.Set<T>().FindAsync(id);
            }
            catch (Exception ex)
            {
                // Handle the exception as needed (e.g., log it)
                throw new Exception("An error occurred while retrieving data.", ex);
            }
        }

        public async Task<T> GetByIdWithSpecAsync(ISpecification<T> spec)
        {

            try
            {
                return await ApplySpecification(spec).FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                // Handle the exception as needed (e.g., log it)
                throw new Exception("An error occurred while retrieving data.", ex);

            }
        }

        public async Task<IEnumerable<T>> GetAllWithSpecAsync(ISpecification<T> spec)
        {
            try
            {
                return await ApplySpecification (spec).ToListAsync();


            }
            catch (Exception ex)
            {
                // Handle the exception as needed (e.g., log it)
                throw new Exception("An error occurred while retrieving data.", ex);
            }
        }

        public IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            return Evalutore<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);

        }
    }

}