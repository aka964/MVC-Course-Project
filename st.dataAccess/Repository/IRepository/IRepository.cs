using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace stDataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(string? includeProperties = null);
        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null);
        public void Add(T item);
        public void Remove(T item);
        public void RemoveRange(IEnumerable<T> entity); 
    }
}
