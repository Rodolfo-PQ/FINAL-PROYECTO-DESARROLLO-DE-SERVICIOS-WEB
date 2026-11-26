using AutoFixManager.Api.Data;
namespace AutoFixManager.Api.Repositories;
public class EfRepository<T>(AutoFixDbContext db) : IRepository<T> where T : class
{
    public IQueryable<T> Query() => db.Set<T>();
    public T? Find(int id) => db.Set<T>().Find(id);
    public void Add(T entity) => db.Set<T>().Add(entity);
    public void Remove(T entity) => db.Set<T>().Remove(entity);
    public void Save() => db.SaveChanges();
}
