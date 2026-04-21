using Pharmacy.Application.Common.Interfaces;

namespace Pharmacy.Infrastructure.Persistence.Repositories
{
    //public class GenericRepository<T> : IRepository<T> where T : class
    //{
    //    private readonly AppDbContext context;

    //    public GenericRepository(AppDbContext context)
    //    {
    //        this.context = context;
    //    }
    //    public T Add(T entity)
    //    {
    //        var newEntity = context.Set<T>().Add(entity);
    //        return newEntity.Entity;
    //    }

    //    public T? Delete(T entity)
    //    {
    //        if (entity == null)
    //            return null;

    //        context.Set<T>().Remove(entity);
    //        return entity;
    //    }

    //    public T? Get(Guid id)
    //    {
    //        return context.Set<T>().Find(id);
    //    }

    //    public IQueryable<T> GetAll()
    //    {
    //        return context.Set<T>().AsNoTracking();
    //    }

    //    public T Update(T entity)
    //    {
    //        var updatedEntity = context.Set<T>().Update(entity);
    //        return updatedEntity.Entity;
    //    }
    //}
}
