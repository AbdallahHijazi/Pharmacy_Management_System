using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Common.Interfaces
{
    public interface IRepository<T> where T : class 
    {
        IQueryable<T> GetAllAsNoTracking();
        T Add(T entity);
        T Update(T entity);
        T? Get(Guid id);
        IQueryable<T> GetAll();
        T? Delete(T entity);
    }
}
