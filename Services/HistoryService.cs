

using Microsoft.EntityFrameworkCore;
using MapsterMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;

using atmnr_api.Reposities;
using atmnr_api.Models;
using atmnr_api.Data;
using atmnr_api.Entities;
// using atmnr_api.Enums;

namespace atmnr_api.Services;

public interface IHistoryService
{
    Task<IEnumerable<HistoryInfo>> Get(HistoryParamsModel id);
    Task<HistoryInfo> Add(HistoryInfo obj);
}
public class HistoryService : GenericService, IHistoryService
{
    protected readonly IMapper _mapper;
    public HistoryService(IHttpContextAccessor http, AtDbContext db, IMapper mapper)
    : base(http, db)
    {
        _mapper = mapper;
    }
    public async Task<IEnumerable<HistoryInfo>> Get(HistoryParamsModel model)
    {
        var query = _db.Histories.Where(f => f.FormId == model.FormId && f.ReferenceId == model.ReferenceId);
        var data = await query.ToListAsync();
        return _mapper.Map<IEnumerable<HistoryInfo>>(data);
    }
    public async Task<HistoryInfo> Add(HistoryInfo obj)
    {
        var entity = new History
        {
            LogId = Guid.NewGuid(),
            Actiondate = DateTime.UtcNow,
            UserId = GetUserId(),
            Actiontype = obj.Actiontype,
            FormId = obj.FormId,
            ReferenceId = obj.ReferenceId,
            Contentlog = obj.Contentlog,
        };
        await _db.Histories.AddAsync(entity);
        await _db.SaveChangesAsync();

        return _mapper.Map<HistoryInfo>(entity);
    }

}

