

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

public interface ITransactionService
{
    Task<TransactionInfo> Get(Guid id);
    Task<IEnumerable<TransactionDetailInfo>> GetDetails(Guid id);
    Task<IEnumerable<TransactionMemberInfo>> GetMembers(Guid id);
    Task<IEnumerable<TransactionInfo>> Query(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryTotal(QueryParamModel model);
    Task<IEnumerable<TransactionInfo>> QueryExpense(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryExpenseTotal(QueryParamModel model);
    Task<IEnumerable<TransactionSummaryInfo>> GetSummary(QueryParamModel model);
    Task<String> GetTransNo(TransactionInfo obj);
    Task<TransactionInfo> Add(TransactionInfo obj);
    Task<TransactionInfo> Update(TransactionInfo obj);
    Task<bool> Delete(Guid id);
    Task<bool> Deletes(Guid[] model);
    Task<IEnumerable<MonthlyDetailInfo>> GetBctcByMonth(MonthlyDetailModel model);
}
public class TransactionService : GenericService, ITransactionService
{
    protected readonly IMapper _mapper;
    readonly GeneralReposity<Transaction> _repoTrans;
    readonly GeneralReposity<TransactionDetail> _repoDetail;
    readonly GeneralReposity<TransactionMember> _repoMember;
    readonly GeneralReposity<Apartment> _repoApartment;
    public TransactionService(IHttpContextAccessor http, AtDbContext db, IMapper mapper)
    : base(http, db)
    {
        _mapper = mapper;
        _repoTrans = new BaseReposity<Transaction>(db, http);
        _repoDetail = new BaseReposity<TransactionDetail>(db, http);
        _repoMember = new BaseReposity<TransactionMember>(db, http);
        _repoApartment = new BaseReposity<Apartment>(db, http);
    }
    public async Task<TransactionInfo> Get(Guid id)
    {
        var query = _repoTrans.Query(f => f.TransId == id);
        var data = await query.FirstOrDefaultAsync();
        return _mapper.Map<TransactionInfo>(data);
    }
    public async Task<IEnumerable<TransactionDetailInfo>> GetDetails(Guid id)
    {
        var query = _repoDetail.Query(f => f.TransId == id);
        var data = await query.ToListAsync();
        return _mapper.Map<IEnumerable<TransactionDetailInfo>>(data);
    }
    public async Task<IEnumerable<TransactionMemberInfo>> GetMembers(Guid id)
    {
        var query = _repoMember.Query(f => f.TransId == id);
        var data = await query.ToListAsync();
        return _mapper.Map<IEnumerable<TransactionMemberInfo>>(data);
    }
    public async Task<IEnumerable<TransactionInfo>> Query(QueryParamModel model, int? page, int? pageSize)
    {
        string[] types = [ETransactionType.PMGB, ETransactionType.PMGT, ETransactionType.PGT];
        var query = ApplyFilter(_repoTrans, model, ["description", "apartment.owner"]).Where(f => types.Contains(f.Transtype));
        query = query.Select(f => new Transaction
        {
            TransId = f.TransId,
            Transno = f.Transno,
            Transtype = f.Transtype,
            Transdate = f.Transdate,
            Status = f.Status,
            ObjectId = f.ObjectId,
            CustomerId = f.CustomerId,
            Apartment = f.Apartment,
            Description = f.Description,
            Totalamount = f.Totalamount,
            Notes = f.Notes,
            CreatedBy = f.CreatedBy,
            CreatedDate = f.CreatedDate,
            UpdatedBy = f.UpdatedBy,
            LastUpdate = f.LastUpdate,
        });

        if (model.Sort == null)
        {
            var sorts = new SortModel[] { new SortModel { Property = "Transdate", Direction = "desc" } };
            query = ApplySort(query, sorts);
        }
        else
            query = ApplySort(query, model.Sort);

        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();


        return _mapper.Map<IEnumerable<TransactionInfo>>(data);
    }

    public async Task<int> QueryTotal(QueryParamModel model)
    {
        string[] types = [ETransactionType.PMGB, ETransactionType.PMGT, ETransactionType.PGT];
        var qry = ApplyFilter(_repoTrans, model, ["description", "apartment.owner"]).Where(f => types.Contains(f.Transtype));
        var c = await qry.CountAsync();
        return c;
    }

    public async Task<IEnumerable<TransactionInfo>> QueryExpense(QueryParamModel model, int? page, int? pageSize)
    {
        string[] types = [ETransactionType.CTL, ETransactionType.CTH, ETransactionType.CTD, ETransactionType.CK];
        var query = ApplyFilter(_repoTrans, model, ["description", "employee.name"]).Where(f => types.Contains(f.Transtype));
        query = query.Select(f => new Transaction
        {
            TransId = f.TransId,
            Transno = f.Transno,
            Transtype = f.Transtype,
            Transdate = f.Transdate,
            Status = f.Status,
            ObjectId = f.ObjectId,
            CustomerId = f.CustomerId,
            Employee = f.Employee,
            Description = f.Description,
            Totalamount = f.Totalamount,
            Notes = f.Notes,
            CreatedBy = f.CreatedBy,
            CreatedDate = f.CreatedDate,
            UpdatedBy = f.UpdatedBy,
            LastUpdate = f.LastUpdate,
        });

        if (model.Sort == null)
        {
            var sorts = new SortModel[] { new SortModel { Property = "Transdate", Direction = "desc" } };
            query = ApplySort(query, sorts);
        }
        else
            query = ApplySort(query, model.Sort);

        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();


        return _mapper.Map<IEnumerable<TransactionInfo>>(data);
    }


    public async Task<int> QueryExpenseTotal(QueryParamModel model)
    {
        string[] types = [ETransactionType.CTL, ETransactionType.CTH, ETransactionType.CTD, ETransactionType.CK];
        var qry = ApplyFilter(_repoTrans, model, ["description", "employee.name"]).Where(f => types.Contains(f.Transtype));
        var c = await qry.CountAsync();
        return c;
    }

    public async Task<TransactionInfo> Add(TransactionInfo obj)
    {
        var entity = await _repoTrans.Create(obj);

        if (string.IsNullOrEmpty(obj.Transno))
        {
            var maxNo = await GetTransNo(obj);
            entity.Transno = maxNo;
        }

        if (obj.Details != null)
        {
            foreach (var d in obj.Details)
            {
                d.TransId = entity.TransId;
                var entityDetail = await _repoDetail.Create(d);
            }
        }

        if (obj.Members != null)
        {
            foreach (var d in obj.Members)
            {
                d.TransId = entity.TransId;
                var entityDetail = await _repoMember.Create(d);
            }
        }

        if (obj.Transtype == ETransactionType.PMGB || obj.Transtype == ETransactionType.PMGT)
        {
            var apartmentEnt = await _repoApartment.FindOne(f => f.ApartmentId == obj.ObjectId && f.Status != EApartmentStatus.DB);
            if (apartmentEnt != null) apartmentEnt.Status = EApartmentStatus.DB;
        }

        await _repoTrans.SaveChanges();
        return _mapper.Map<TransactionInfo>(entity);
    }

    public async Task<TransactionInfo> Update(TransactionInfo obj)
    {
        var entity = await _repoTrans.FindByIdAndUpdate(obj, obj.TransId);

        if (obj.Details != null)
        {
            decimal sumAmount = 0;
            foreach (var d in obj.Details)
            {
                var entDetail = await _repoDetail.FindOneAndUpdate(f => f.TransId == entity.TransId && f.Linenum == d.Linenum, d);
                if (entDetail == null)
                {
                    d.TransId = entity.TransId;
                    entDetail = await _repoDetail.Create(d);

                }
                sumAmount += entDetail.Amount;
            }
            if (sumAmount == entity.Totalamount) entity.Status = ETransactionStatus.D;
        }
        if (obj.D_Details != null)
        {
            foreach (var itemDelete in obj.D_Details)
            {
                await _repoDetail.FindOneAndDelete(f => f.TransId == entity.TransId && f.Linenum == itemDelete.Linenum);
            }
        }

        if (obj.Members != null)
        {
            foreach (var d in obj.Members)
            {
                var entDetail = await _repoMember.FindOneAndUpdate(f => f.TransId == entity.TransId && f.UserId == d.UserId, d);
                if (entDetail == null)
                {
                    d.TransId = entity.TransId;
                    var entityDetail = await _repoMember.Create(d);

                }
            }
        }
        if (obj.D_Members != null)
        {
            foreach (var itemDelete in obj.D_Members)
            {
                await _repoMember.FindOneAndDelete(f => f.TransId == entity.TransId && f.UserId == itemDelete.UserId);
            }
        }

        if (entity.Transtype == ETransactionType.PMGB || entity.Transtype == ETransactionType.PMGT)
        {
            var apartmentEnt = await _repoApartment.FindOne(f => f.ApartmentId == entity.ObjectId);
            if (apartmentEnt != null)
            {
                if (entity.Status == ETransactionStatus.H) apartmentEnt.Status = EApartmentStatus.HD;
                else apartmentEnt.Status = EApartmentStatus.DB;
            }
        }

        await _repoTrans.SaveChanges();
        return _mapper.Map<TransactionInfo>(entity);
    }

    public async Task<IEnumerable<TransactionInfo>> ValidDelete(Guid[] ids)
    {
        List<TransactionInfo> items = new List<TransactionInfo>();
        foreach (var id in ids)
        {
            var entity = await _repoTrans.FindById(id);
            // MakeSure(entity != null, "NOT_FOUND");
            // var item = _mapper.Map<PayItemInfo>(entity);
            items.Add(_mapper.Map<TransactionInfo>(entity));
            // var payrollEnt = await _repoPayrollDetail.Query(f => f.NbpayitemId == id).FirstOrDefaultAsync();
            // MakeSure(payrollEnt == null, "DELETE_EXISTS", new { field0 = entity.Itemname, field1 = "form.BPR" });
            // var payslipEnt = await _repoPayslipDetail.Query(f => f.NbpayitemId == id).FirstOrDefaultAsync();
            // MakeSure(payslipEnt == null, "DELETE_EXISTS", new { field0 = entity.Itemname, field1 = "form.BPR" });

        }
        return items;
    }

    public async Task<bool> Delete(Guid id)
    {
        var entity = await _repoTrans.FindByIdAndDelete(id);
        if (entity.Transtype == ETransactionType.PMGB || entity.Transtype == ETransactionType.PMGT)
        {
            var apartmentEnt = await _repoApartment.FindOne(f => f.ApartmentId == entity.ObjectId && f.Status == EApartmentStatus.DB);
            if (apartmentEnt != null) apartmentEnt.Status = EApartmentStatus.HD;
        }
        await _repoDetail.DeleteMany(f => f.TransId == id);
        await _repoTrans.SaveChanges();
        return true;
    }
    public async Task<bool> Deletes(Guid[] ids)
    {
        var items = await ValidDelete(ids);
        foreach (var item in items)
        {

            var entity = await _repoTrans.FindByIdAndDelete(item.TransId);
            if (entity.Transtype == ETransactionType.PMGB || entity.Transtype == ETransactionType.PMGT)
            {
                var apartmentEnt = await _repoApartment.FindOne(f => f.ApartmentId == entity.ObjectId && f.Status == EApartmentStatus.DB);
                if (apartmentEnt != null) apartmentEnt.Status = EApartmentStatus.HD;
            }
        }
        await _repoTrans.SaveChanges();
        return true;
    }

    public async Task<IEnumerable<TransactionSummaryInfo>> GetSummary(QueryParamModel model)
    {
        var qry = ApplyFilter(_repoTrans, model, ["transno"], "TransId");
        var data = await qry.GroupBy(x => x.Transtype).Select(g => new
        TransactionSummaryInfo
        {
            Key = g.Key.ToString(),
            Count = g.Count()
        }).ToListAsync();

        return _mapper.Map<IEnumerable<TransactionSummaryInfo>>(data);
    }


    public async Task<String> GetTransNo(TransactionInfo obj)
    {
        string sYear = (DateTime.Now.Year % 100).ToString();
        string maxNo = "";
        String[] arrRevenueType = new String[] { ETransactionType.PMGB, ETransactionType.PMGT };

        if (obj.Transtype == ETransactionType.PMGB || obj.Transtype == ETransactionType.PMGT)
        {
            var count = await _repoTrans.Query(f => arrRevenueType.Contains(f.Transtype)).CountAsync() + 1;
            maxNo = "HĐMGCH/" + sYear + count.ToString();
        }

        return maxNo;
    }

    public async Task<IEnumerable<MonthlyDetailInfo>> GetBctcByMonth(MonthlyDetailModel model)
    {
        string[] types = model.Revenuetype == "T" ? [ETransactionType.PMGB, ETransactionType.PMGT, ETransactionType.PGT] : [ETransactionType.CTL, ETransactionType.PMGT, ETransactionType.PGT, ETransactionType.CK];
        DateTime utcNow = DateTime.UtcNow;
        DateTime firstDay = new DateTime(utcNow.Year, model.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime lastDay = new DateTime(utcNow.Year, model.Month, DateTime.DaysInMonth(utcNow.Year, model.Month), 23, 59, 59, DateTimeKind.Utc);

        var data = await _repoDetail.Query(f => f.Date >= firstDay && f.Date <= lastDay)
        .Join(_db.Transactions, d => d.TransId, t => t.TransId, (d, t) => new { d, t })
        .Where(p => types.Contains(p.t.Transtype)).Select(f => new MonthlyDetailInfo
        {
            TransId = f.d.TransId,
            Date = f.d.Date,
            Linenum = f.d.Linenum,
            Description = f.t.Description,
            Amount = f.d.Amount
        }).ToListAsync();

        return data;
    }
}

