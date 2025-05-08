using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using atmnr_api.Data;
using atmnr_api.Entities;
using System.Security.Claims;

namespace atmnr_api.Reposities;

public class BaseReposity<TEntity> : GeneralReposity<TEntity>
where TEntity : BaseEntity
{
    protected readonly String _userId;
    IHttpContextAccessor _http;
    public BaseReposity(AtDbContext db, IHttpContextAccessor http) : base(db)
    {
        _http = http;
        string sub = _http.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        if (string.IsNullOrEmpty(sub)) _userId = "anonymous";
        else _userId = sub;
    }
    public BaseReposity(AtDbContext db, String userId) : base(db)
    {
        _userId = userId;
    }
    public override async Task<TEntity> Create(object input)
    {
        var entity = Activator.CreateInstance<TEntity>();

        entity = ApplyChanges(entity, input);
        entity.CreatedBy = _userId;
        entity.UpdatedBy = _userId;
        entity.LastUpdate = DateTime.UtcNow;
        entity.CreatedDate = DateTime.UtcNow;
        var entityEntry = await _dbSet.AddAsync(entity);
        return entityEntry.Entity;
    }
    public override async Task CreateMany(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.CreatedBy = _userId;
            entity.UpdatedBy = _userId;
            entity.LastUpdate = DateTime.UtcNow;
            entity.CreatedDate = DateTime.UtcNow;
        }

        await base.CreateMany(entities);
    }

    public override async Task<TEntity> FindByIdAndUpdate(object input, params object?[]? keyValues)
    {
        var entity = await Table.FindAsync(keyValues);
        if (entity == null) return null;
        return ApplyChanges(entity, input);
    }
    public override async Task<TEntity> FindOneAndUpdate(Expression<Func<TEntity, bool>> filters, object input)
    {
        var entity = await Table.FirstOrDefaultAsync(filters);
        if (entity == null) return null;

        return ApplyChanges(_db.Entry(entity).Entity, input);
    }
    public override async Task<int> UpdateMany(Expression<Func<TEntity, bool>> filters, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls)
    {
        return await base.UpdateMany(filters, setPropertyCalls);

    }
    public override TEntity ApplyChanges(TEntity entity, object input)
    {
        entity = base.ApplyChanges(entity, input);
        entity.UpdatedBy = _userId;
        entity.LastUpdate = DateTime.UtcNow;
        return entity;
    }

}