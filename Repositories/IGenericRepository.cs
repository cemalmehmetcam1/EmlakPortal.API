namespace EmlakPortal.API.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<int> SaveAsync();

        // Gelişmiş sorgular (Include, Where vs.) için query yeteneği
        IQueryable<T> AsQueryable();
    }
}