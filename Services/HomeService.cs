

using Microsoft.EntityFrameworkCore;
using MapsterMapper;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqKit;

using atmnr_api.Reposities;
using atmnr_api.Models;
using atmnr_api.Data;
using atmnr_api.Entities;
using atmnr_api.Helpers;
using atmnr_api.Enums;

namespace atmnr_api.Services;

public interface IHomeService
{
    Task<IEnumerable<MonthlyRevenueByUserInfo>> GetRevenueMonthly(MonthlyRevenueByUserModel model);
}
public class HomeService : GenericService, IHomeService
{
    protected readonly IMapper _mapper;
    readonly GeneralReposity<Transaction> _repoTrans;
    readonly GeneralReposity<TransactionDetail> _repoDetail;
    readonly GeneralReposity<TransactionMember> _repoMember;
    public HomeService(IHttpContextAccessor http, AtDbContext db, IMapper mapper)
    : base(http, db)
    {
        _mapper = mapper;
        _repoTrans = new BaseReposity<Transaction>(db, http);
        _repoDetail = new BaseReposity<TransactionDetail>(db, http);
        _repoMember = new BaseReposity<TransactionMember>(db, http);
    }
    public async Task<IEnumerable<MonthlyRevenueByUserInfo>> GetRevenueMonthly(MonthlyRevenueByUserModel model)
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime firstDay = new DateTime(utcNow.Year, model.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime lastDay = new DateTime(utcNow.Year, model.Month, DateTime.DaysInMonth(utcNow.Year, model.Month), 23, 59, 59, DateTimeKind.Utc);
        var data = await _repoDetail.Query(f => f.Date >= firstDay && f.Date <= lastDay)
        .Join(_db.Transactions, d => d.TransId, t => t.TransId, (d, t) => new { d, t })
        .Join(_db.TransactionMembers, master => master.d.TransId, m => m.TransId, (master, m) => new MonthlyRevenueByUserInfo
        {
            TransId = master.d.TransId,
            Linenum = master.d.Linenum,
            Date = master.d.Date,
            Amount = master.d.Amount,
            Rate = m.Rate,
            Description = master.t.Description,
            UserId = m.UserId,
        }).Where(f => f.UserId == model.UserId).ToListAsync();

        return _mapper.Map<IEnumerable<MonthlyRevenueByUserInfo>>(data);
    }
}

