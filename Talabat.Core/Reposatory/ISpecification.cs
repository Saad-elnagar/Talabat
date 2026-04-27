using System.Linq.Expressions;
using Talabat.Core.Entitys;

namespace Talabat.Reposatory.Specification
{
    public interface ISpecification<T> where T : Base
    {
        public Expression<Func<T, bool>> Criteria { get; }
         List<Expression<Func<T, object>>> Includes { get; }


    }
}