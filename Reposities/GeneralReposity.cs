using Microsoft.EntityFrameworkCore;
using atmnr_api.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
// using neo_api.Models.PO;
// using neo_api.Helpers;

namespace atmnr_api.Reposities;

public interface IGeneralReposity<TEntity>
    where TEntity : class
{
    Task<bool> Any(Expression<Func<TEntity, bool>> predicate);
    Task<IEnumerable<TEntity>> FindAll(Expression<Func<TEntity, bool>> predicate, int page = 0, int pageSize = 0);
    Task<IEnumerable<TResult>> FindAllTo<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, int page = 0, int pageSize = 0);
    IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Finds an entity by its ID.
    /// </summary>
    /// <param name="keyValues">The IDs of Entity.</param>
    /// <returns>The entity with the specified ID, or null if not found.</returns>
    Task<TEntity?> FindById(params object?[]? keyValues);
    Task<TEntity?> FindOne(Expression<Func<TEntity, bool>> filters);
    Task<TEntity> Create(object entity);

    Task<TEntity> FindByIdAndUpdate(object data, params object?[]? keyValues);
    Task<TEntity> FindOneAndUpdate(Expression<Func<TEntity, bool>> filters, object data);
    Task<TEntity?> FindByIdAndDelete(params object?[]? keyValues);
    Task<TEntity?> FindOneAndDelete(Expression<Func<TEntity, bool>> filters);
    Task CreateMany(IEnumerable<TEntity> entities);
    Task<int> UpdateMany(Expression<Func<TEntity, bool>> filters, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls);
    Task<int> DeleteMany(Expression<Func<TEntity, bool>> filters);
    Task<int> Count(Expression<Func<TEntity, bool>> predicate);
    Task<decimal> Sum(Expression<Func<TEntity, bool>> filters, string property);
    Task<int> MaxInt(Expression<Func<TEntity, bool>> filters, string property);
    // Task<DateTime> MaxDate(Expression<Func<TEntity, bool>> filters, string property);
    Task<long> MaxInt64(Expression<Func<TEntity, bool>> filters, string property);
    Task<short> MaxShort(Expression<Func<TEntity, bool>> filters, string property);
    Task<int> SaveChanges(CancellationToken cancellationToken = default);
    DbSet<TEntity> GetTable();
    TEntity ApplyChanges(TEntity entity, object input);
}
public class GeneralReposity<TEntity> : IGeneralReposity<TEntity>
    where TEntity : class
{
    protected readonly AtDbContext _db;
    protected readonly DbSet<TEntity> _dbSet;
    public DbSet<TEntity> Table => _dbSet;
    public DbSet<TEntity> GetTable() => _dbSet;
    public GeneralReposity(AtDbContext db)
    {
        _db = db;
        _dbSet = _db.Set<TEntity>();
    }
    public virtual async Task<bool> Any(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }
    public virtual async Task<int> Count(Expression<Func<TEntity, bool>> predicate)
    {

        return await _dbSet.CountAsync(predicate);
    }

    public virtual async Task<TEntity> Create(object input)
    {
        var entity = Activator.CreateInstance<TEntity>();
        entity = ApplyChanges(entity, input);
        var entityEntry = await _dbSet.AddAsync(entity);
        return entityEntry.Entity;
    }

    public virtual async Task CreateMany(IEnumerable<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public virtual async Task<int> DeleteMany(Expression<Func<TEntity, bool>> filters)
    {
        return await _dbSet.Where(filters).ExecuteDeleteAsync();
    }

    public virtual async Task<TEntity?> FindById(params object?[]? keyValues)
    {
        var entity = await _dbSet.FindAsync(keyValues);
        return entity ?? null;
    }
    public virtual async Task<TEntity?> FindOne(Expression<Func<TEntity, bool>> filters)
    {
        var entity = await _dbSet.Where(filters).FirstOrDefaultAsync();
        return entity ?? null;
        // if (entity == null) return null;
        // return _db.Entry(entity).Entity;
    }

    public virtual async Task<TEntity?> FindByIdAndDelete(params object?[]? keyValues)
    {
        var entity = await _dbSet.FindAsync(keyValues);
        if (entity == null) return null;
        _dbSet.Remove(entity);
        return entity;
    }

    public virtual async Task<TEntity> FindByIdAndUpdate(object input, params object?[]? keyValues)
    {
        var entity = await _dbSet.FindAsync(keyValues);
        if (entity == null) return null;
        return ApplyChanges(entity, input);

    }

    public virtual async Task<TEntity?> FindOneAndDelete(Expression<Func<TEntity, bool>> filters)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(filters);

        if (entity == null) return null;
        _dbSet.Remove(entity); //_db.Entry(entity).Entity
        return entity;
    }

    public virtual async Task<TEntity> FindOneAndUpdate(Expression<Func<TEntity, bool>> filters, object input)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(filters);
        if (entity == null) return null;
        // _db.Entry(entity).Entity
        return ApplyChanges(entity, input);
    }

    public Task<decimal> Sum(Expression<Func<TEntity, bool>> filters, string property)
    {
        return _dbSet.Where(filters).SumAsync(f => EF.Property<decimal>(f, property));
    }
    // public virtual async Task<DateTime> MaxDate(Expression<Func<TEntity, bool>> filters, string property)
    // {
    //     return await _dbSet.Where(filters)
    //     .MaxAsync(f => (DateTime?)EF.Property<object>(f, property)) ?? Constants.MinDate;
    // }
    public virtual async Task<int> MaxInt(Expression<Func<TEntity, bool>> filters, string property)
    {
        return await _dbSet.Where(filters)
        .MaxAsync(f => (int?)EF.Property<object>(f, property)) ?? 0;
    }
    public virtual async Task<short> MaxShort(Expression<Func<TEntity, bool>> filters, string property)
    {
        return await _dbSet.Where(filters)
        .MaxAsync(f => (short?)EF.Property<object>(f, property)) ?? 0;
    }
    public virtual async Task<long> MaxInt64(Expression<Func<TEntity, bool>> filters, string property)
    {
        return await _dbSet.Where(filters)
        .MaxAsync(f => (long?)EF.Property<object>(f, property)) ?? 0;
    }
    public virtual async Task<int> UpdateMany(Expression<Func<TEntity, bool>> filters, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls)
    {
        return await _dbSet.Where(filters).ExecuteUpdateAsync(setPropertyCalls);
    }
    public virtual async Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return await _db.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> FindAll(Expression<Func<TEntity, bool>> filters, int page = 0, int pageSize = 0)
    {
        var query = _dbSet.Where(filters);
        if (pageSize > 0) query = query.Skip(page * pageSize).Take(pageSize);
        return await query.ToArrayAsync();
    }
    public virtual async Task<IEnumerable<TResult>> FindAllTo<TResult>(Expression<Func<TEntity, bool>> filters, Expression<Func<TEntity, TResult>> selector, int page = 0, int pageSize = 0)
    {
        var query = _dbSet.Where(filters).Select(selector);
        if (pageSize > 0) query = query.Skip(page * pageSize).Take(pageSize);
        return await query.ToArrayAsync();
    }
    public virtual TEntity ApplyChanges(TEntity entity, object input)
    {
        var entry = _db.Entry(entity);
        foreach (var property in entry.CurrentValues.Properties)
        {
            var iProps = input.GetType().GetProperty(property.Name);
            if (iProps != null)
            {
                var newValue = iProps.GetValue(input);

                if (newValue != null)
                {
                    if (property.GetType() is Guid? && Guid.TryParse(newValue + "", out Guid guidValue) && guidValue == Guid.Empty)
                    {
                        entry.Property(property.Name).CurrentValue = null;
                    }
                    else entry.Property(property.Name).CurrentValue = newValue;
                }
            }
        }
        // entity.UpdatedBy = _userId;
        // entity.LastUpdate = DateTime.Now;
        return entity;
    }

    public virtual IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }
}