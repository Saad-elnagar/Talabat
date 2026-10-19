using System.Linq.Expressions;
using Talabat.Domain.Entities;
using Talabat.Domain.Specifications;

namespace Talabat.Domain.Specifications
{
    public interface ISpecification<T> where T : BaseEntity
    {
        public Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        Expression<Func<T, object>> OrderBy { get; set; }
        Expression<Func<T, object>> OrderByDescending { get; set; }
      
        public int Skip { get; set; }
        public int Take { get; set; }
        bool IsPagingEnabled { get; }
    }
}