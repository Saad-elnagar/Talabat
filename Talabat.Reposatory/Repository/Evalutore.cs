using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys;
using Talabat.Reposatory.Specification;

namespace Talabat.Reposatory.Repository
{
    public static class Evalutore<TEntity> where TEntity : Base
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> spec)
        {

            if (spec.Criteria != null)
            {
                inputQuery = inputQuery.Where(spec.Criteria);
            }
            if (spec.Includes != null)
            {
                inputQuery = spec.Includes.Aggregate(inputQuery, (current, include) => current.Include(include));
            }
            return inputQuery;
        }

    }
}
