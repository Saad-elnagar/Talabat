using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys;

namespace Talabat.Core.Reposatory
{
    public interface IGenericRepository<T> where T : Base
    {

        Task<T> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
     
    }
}
