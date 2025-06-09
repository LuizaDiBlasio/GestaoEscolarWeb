using System.Linq;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll(); // devolve todas os objetos de uma dada classe

        Task<T> GetByIdAsync(int id); //Id definido pelo IEntity

        Task CreateAsync(T entity); //cria uma entidade qualquer

        Task UpdateAsync(T entity); //faz update de uma entidade qualquer

        Task DeleteAsync(T entity);  // deleta uma entidade qualquer

        Task<bool> ExistAsync(int id); // ver se existem objetos de uma entitade qualquer
    }
}
