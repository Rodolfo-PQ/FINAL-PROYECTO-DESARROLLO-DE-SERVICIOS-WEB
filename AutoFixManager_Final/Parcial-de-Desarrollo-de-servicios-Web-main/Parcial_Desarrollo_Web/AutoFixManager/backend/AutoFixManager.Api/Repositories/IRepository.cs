namespace AutoFixManager.Api.Repositories;
public interface IRepository<T> where T : class { IQueryable<T> Query(); T? Find(int id); void Add(T entity); void Remove(T entity); void Save(); }
