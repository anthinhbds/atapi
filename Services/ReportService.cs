

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

public interface IReportService
{
    Task<IEnumerable<ViewStatisticInfo>> GetTotalView();
}
public class ReportService : GenericService, IReportService
{
    protected readonly IMapper _mapper;
    public ReportService(IHttpContextAccessor http, AtDbContext db, IMapper mapper)
    : base(http, db)
    {
        _mapper = mapper;
    }
    public async Task<IEnumerable<ViewStatisticInfo>> GetTotalView()
    {
        DateTime endDate = DateTime.UtcNow;
        DateTime startDate = endDate.AddDays(-7);
        var query = _db.ViewStatistics.Where(f => f.Createddate >= startDate && f.Createddate <= endDate);
        var data = await query.ToListAsync();
        return _mapper.Map<IEnumerable<ViewStatisticInfo>>(data);
    }
}

