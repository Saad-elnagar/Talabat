using Microsoft.EntityFrameworkCore;
using System.Linq;
using Talabat.Domain.Entities;
using Talabat.Domain.Specifications;

namespace Talabat.Infrastructure.Repositories
{
    public static class Evalutore<TEntity> where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> spec)
        {
            if (spec.Criteria != null)
            {
                inputQuery = inputQuery.Where(spec.Criteria);
            }

            if (spec.OrderBy != null)
            {
                inputQuery = inputQuery.OrderBy(spec.OrderBy);
            }

            if (spec.OrderByDescending != null)
            {
                inputQuery = inputQuery.OrderByDescending(spec.OrderByDescending);
            }

            if (spec.Includes != null)
            {
                inputQuery = spec.Includes.Aggregate(inputQuery, (current, include) => current.Include(include));
            }

            if (spec.IsPagingEnabled)
            {
                inputQuery = inputQuery.Skip(spec.Skip).Take(spec.Take);
            }

            return inputQuery;
        }
    }
}