using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Talabat.Core.Entitys;

namespace Talabat.Reposatory.Specification
{
    public class Specification<T> : ISpecification<T> where T : Base
    {
        public Expression<Func<T, bool>> Criteria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set; } = new List<Expression<Func<T, object>>>();
        public Expression<Func<T, object>> OrderBy { get; set; }
        public Expression<Func<T, object>> OrderByDescending { get; set; }
        


        public bool IsPagingEnabled { get; private set; }
        public int Skip { get; set; }
        public int Take { get; set; }
      

        public Specification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        public Specification()
        {
        }

        public void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        public void AddOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
        }
        public void ApplyPagination(int skip, int take)
        {
            IsPagingEnabled = true;
            Skip = skip;
            Take = take;
        }
    }
}