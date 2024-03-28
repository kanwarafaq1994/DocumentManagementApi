using DocumentManagement.Data.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagement.Data.Repositories
{
    public interface IRepository<T> where T : class, IEntity
    {
        Task<List<T>> GetAll();
        Task<T> Get(int id);
        Task<T> Add(T entity);
        void AddRange(List<T> entities);
        void DeleteRange(List<T> entities);
        Task<T> Delete(int id);
        Task ReloadAsync(T entity);
        Task<int> CountAll();
        public Task<T> Update(T entity);
        public Task<InfoDto> Validate(T entity);
    }
}
