using System.Linq.Expressions;
using Talabat.Core.Entitys;

namespace Talabat.Reposatory.Specification
{
    public interface ISpecification<T> where T : Base
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