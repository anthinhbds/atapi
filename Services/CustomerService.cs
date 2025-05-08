

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

public interface ICustomerService
{
    Task<CustomerInfo> Get(string id);
    Task<IEnumerable<CustomerNoteInfo>> GetNotes(string id);
    Task<IEnumerable<CustomerInfo>> Query(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryTotal(QueryParamModel model);
    Task<IEnumerable<CustomerInfo>> QueryAll(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryAllTotal(QueryParamModel model);
    Task<IEnumerable<CustomerInfo>> Combo(QueryParamModel model, int? page, int? pageSize);
    Task<int> ComboTotal(QueryParamModel model);
    Task<IEnumerable<CustomerSummaryInfo>> GetSummary(QueryParamModel model);
    Task<CustomerInfo> Add(CustomerInfo obj);
    Task<CustomerInfo> Update(CustomerInfo obj);
    // Task<bool> Archive(string[] ids);
    // Task<bool> ArchiveAll();
    // Task<bool> Restore(string[] ids);
    // Task<bool> RestoreAll();
    Task<bool> Delete(String id);
    Task<bool> Deletes(String[] model);
    Task<bool> UserDelete(String id);
    Task<bool> UserDeletes(String[] model);
    Task<bool> AssignmenCustomer(AssignmentAaprtmentModel model);
    Task<IEnumerable<AssignmentLogInfo>> GetAssignment(string id);
    Task<TelephoneExistsInfo> IsTelephoneExists(string apartmentId, string[] phones);
}
public class CustomerService : GenericService, ICustomerService
{
    protected readonly IMapper _mapper;
    readonly GeneralReposity<Customer> _repo;
    readonly GeneralReposity<CustomerNote> _repoNote;
    readonly GeneralReposity<AssignmentLog> _repoAssLog;

    public CustomerService(IHttpContextAccessor http, AtDbContext db, IMapper mapper)
    : base(http, db)
    {
        _mapper = mapper;
        _repo = new BaseReposity<Customer>(db, http);
        _repoNote = new BaseReposity<CustomerNote>(db, http);
        _repoAssLog = new BaseReposity<AssignmentLog>(db, http);
    }
    public async Task<CustomerInfo> Get(string id)
    {
        var query = _repo.Query(f => f.CustomerId == id);
        var data = await query.FirstOrDefaultAsync();
        return _mapper.Map<CustomerInfo>(data);
    }
    public async Task<IEnumerable<CustomerNoteInfo>> GetNotes(string id)
    {
        var query = _repoNote.Query(f => f.CustomerId == id && f.Type == "N");
        var data = await query.ToListAsync();
        return _mapper.Map<IEnumerable<CustomerNoteInfo>>(data);
    }
    public async Task<IEnumerable<CustomerInfo>> Query(QueryParamModel model, int? page, int? pageSize)
    {
        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
            await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
            await HasPermit(EClaimId.Employee, EClaimValue.Leader);

        string[] states = ["W", "HD"];
        var query = ApplyFilter(_repo, model, ["customername", "telephone", "telephone2", "telephone3", "notes"]).Where(f => states.Contains(f.Status));

        if (!isAdmin) query = query.Where(f => f.UserId == GetUserId());

        query = query.Select(item => new Customer
        {
            CustomerId = item.CustomerId,
            Customername = item.Customername,
            Status = item.Status,
            // Telephone = (isAdmin || userQuery != null) ? item.Telephone : item.Telephone.Substring(item.Telephone.Length - 5),
            // Telephone2 = (isAdmin || userQuery != null) ? item.Telephone2 : item.Telephone2.Substring(item.Telephone2.Length - -5),
            // Telephone3 = (isAdmin || userQuery != null) ? item.Telephone3 : item.Telephone3.Substring(item.Telephone3.Length - -5),
            // Telephone4 = (isAdmin || userQuery != null) ? item.Telephone4 : item.Telephone4.Substring(item.Telephone4.Length - -5),
            Telephone = item.Telephone,
            Telephone2 = item.Telephone2,
            Telephone3 = item.Telephone3,
            Telephone4 = item.Telephone4,
            Demand = item.Demand,
            Arearange = item.Arearange,
            Pricerange = item.Pricerange,
            Bedroom = item.Bedroom,
            Priority = item.Priority,
            UserId = item.UserId,
            PrevioususerId = item.PrevioususerId,
            Notes = item.Notes,
            Leadsource = item.Leadsource,
            Leadsourceother = item.Leadsourceother,
            Project = item.Project,
            Projectother = item.Projectother,
            Furniture = item.Furniture,
            CreatedBy = item.CreatedBy,
            CreatedDate = item.CreatedDate,
            UpdatedBy = item.UpdatedBy,
            LastUpdate = item.LastUpdate,
            Sortdate = item.LastUpdate
        });


        if (model.Sort == null)
        {
            var sorts = new SortModel[] { new SortModel { Property = "Priority", Direction = "desc" }, new SortModel { Property = "Sortdate", Direction = "desc" } };
            query = ApplySort(query, sorts);
        }
        else
            query = ApplySort(query, model.Sort);

        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();


        return _mapper.Map<IEnumerable<CustomerInfo>>(data);
    }


    public async Task<int> QueryTotal(QueryParamModel model)
    {
        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
           await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
           await HasPermit(EClaimId.Employee, EClaimValue.Leader);

        string[] states = ["W", "HD"];
        var query = ApplyFilter(_repo, model, ["customername", "telephone", "telephone2", "telephone3", "notes"]).Where(f => states.Contains(f.Status));

        if (!isAdmin) query = query.Where(f => f.UserId == GetUserId());

        var c = await query.CountAsync();
        return c;
    }

    public async Task<IEnumerable<CustomerInfo>> Combo(QueryParamModel model, int? page, int? pageSize)
    {
        var displaycustomer = new FilterModel();
        if (model.Filter != null)
        {
            displaycustomer = model.Filter?.FirstOrDefault(f => f.Property == "displaycustomer");
            model.Filter = model.Filter.Where(f => f.Property != "displaycustomer");
        }

        var query = ApplyFilter(_repo, model, ["customername", "telephone", "telephone2", "telephone3"]);

        if (displaycustomer != null && displaycustomer.Property == "displaycustomer")
        {

            string v = displaycustomer.Value.ToString();
            query = query.Where(f => (f.Customername + " - " + f.Telephone).ToUpper().Contains(v.ToUpper()));
        }

        query = query.Select(item => new Customer
        {
            CustomerId = item.CustomerId,
            Customername = item.Customername,
            Telephone = item.Telephone,
            Telephone2 = item.Telephone2,
            Displaycustomer = item.Customername + (item.Telephone != null ? " - " + item.Telephone : "") + (item.Telephone2 != null ? " - " + item.Telephone2 : ""),

        });


        if (model.Sort != null)
        {
            query = ApplySort(query, model.Sort);
        }


        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();


        return _mapper.Map<IEnumerable<CustomerInfo>>(data);
    }

    public async Task<int> ComboTotal(QueryParamModel model)
    {
        var displaycustomer = new FilterModel();
        if (model.Filter != null)
        {
            displaycustomer = model.Filter?.FirstOrDefault(f => f.Property == "displaycustomer");
            model.Filter = model.Filter.Where(f => f.Property != "displaycustomer");
        }

        var query = ApplyFilter(_repo, model, ["customername", "telephone", "telephone2", "telephone3"]);

        if (displaycustomer != null && displaycustomer.Property == "displaycustomer")
        {

            string v = displaycustomer.Value.ToString();
            query = query.Where(f => (f.Customername + " - " + f.Telephone).ToUpper().Contains(v.ToUpper()));
        }

        var c = await query.CountAsync();
        return c;
    }


    public async Task<IEnumerable<CustomerInfo>> QueryAll(QueryParamModel model, int? page, int? pageSize)
    {
        var displaycustomer = new FilterModel();
        if (model.Filter != null)
        {
            displaycustomer = model.Filter?.FirstOrDefault(f => f.Property == "displaycustomer");
            model.Filter = model.Filter.Where(f => f.Property != "displaycustomer");
        }

        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
            await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
            await HasPermit(EClaimId.Employee, EClaimValue.Leader);

        string[] states = ["W", "HD"];
        var query = ApplyFilter(_repo, model, ["customername", "telephone", "telephone2", "telephone3", "notes"]).Where(f => states.Contains(f.Status));

        if (displaycustomer != null && displaycustomer.Property == "displaycustomer")
        {

            string v = displaycustomer.Value.ToString();
            // query = query.Where(f => f.Accountcode.Contains(v) || f.Accountname.Contains(v));
            query = query.Where(f => (f.Customername + " - " + (isAdmin ? f.Telephone : f.Telephone.Substring(f.Telephone.Length - 5))).ToUpper().Contains(v.ToUpper()));
        }

        query = query.Select(item => new Customer
        {
            CustomerId = item.CustomerId,
            Customername = item.Customername,
            Displaycustomer = item.Customername + " - " + (isAdmin ? item.Telephone : item.Telephone.Substring(item.Telephone.Length - 5)),
            Status = item.Status,
            Telephone = isAdmin ? item.Telephone : item.Telephone.Substring(item.Telephone.Length - 5),
            Telephone2 = isAdmin ? item.Telephone2 : item.Telephone2.Substring(item.Telephone2.Length - -5),
            Telephone3 = isAdmin ? item.Telephone3 : item.Telephone3.Substring(item.Telephone3.Length - -5),
            Telephone4 = isAdmin ? item.Telephone4 : item.Telephone4.Substring(item.Telephone4.Length - -5),
            Demand = item.Demand,
            Arearange = item.Arearange,
            Pricerange = item.Pricerange,
            Bedroom = item.Bedroom,
            Priority = item.Priority,
            UserId = item.UserId,
            PrevioususerId = item.PrevioususerId,
            Notes = item.Notes,
            Leadsource = item.Leadsource,
            Leadsourceother = item.Leadsourceother,
            Project = item.Project,
            Projectother = item.Projectother,
            Furniture = item.Furniture,
            CreatedBy = item.CreatedBy,
            CreatedDate = item.CreatedDate,
            UpdatedBy = item.UpdatedBy,
            LastUpdate = item.LastUpdate,
            Sortdate = item.LastUpdate
        });


        if (model.Sort == null)
        {
            var sorts = new SortModel[] { new SortModel { Property = "Priority", Direction = "desc" }, new SortModel { Property = "Sortdate", Direction = "desc" } };
            query = ApplySort(query, sorts);
        }
        else
            query = ApplySort(query, model.Sort);

        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();


        return _mapper.Map<IEnumerable<CustomerInfo>>(data);
    }

    public async Task<int> QueryAllTotal(QueryParamModel model)
    {
        string[] states = ["W", "HD"];
        var query = ApplyFilter(_repo, model, ["customername", "telephone", "telephone2", "telephone3", "notes"]).Where(f => states.Contains(f.Status));
        var c = await query.CountAsync();
        return c;
    }


    public async Task<CustomerInfo> Add(CustomerInfo obj)
    {
        var maxNo = await _repo.Query(b => b.CustomerId.StartsWith("C") && Regex.IsMatch(b.CustomerId.Substring(1), $@"^\d{{{9}}}$"))
            .MaxAsync(b => b.CustomerId);

        if (maxNo == null) maxNo = "C" + 1.ToString($"D{9}");
        else
        {
            int max = int.Parse(maxNo.Substring(maxNo.Length - 9)) + 1;
            maxNo = "C" + max.ToString($"D{9}");
        }
        var entity = await _repo.Create(obj);
        entity.CustomerId = maxNo;

        if (obj.Details != null)
        {
            foreach (var d in obj.Details)
            {
                d.CustomerId = entity.CustomerId;
                var entityDetail = await _repoNote.Create(d);
            }
        }

        await _repo.SaveChanges();
        return _mapper.Map<CustomerInfo>(entity);
    }

    public async Task<CustomerInfo> Update(CustomerInfo obj)
    {
        var entity = await _repo.FindByIdAndUpdate(obj, obj.CustomerId);

        if (obj.Details != null)
        {
            foreach (var d in obj.Details)
            {
                var entDetail = await _repoNote.FindOneAndUpdate(f => f.CustomerId == entity.CustomerId && f.Linenum == d.Linenum, d);
                if (entDetail == null)
                {
                    d.CustomerId = entity.CustomerId;
                    var entityDetail = await _repoNote.Create(d);

                }
            }
        }
        if (obj.D_Details != null)
        {
            foreach (var itemDelete in obj.D_Details)
            {
                await _repoNote.FindOneAndDelete(f => f.CustomerId == entity.CustomerId && f.Linenum == itemDelete.Linenum);
            }
        }

        await _repo.SaveChanges();
        return _mapper.Map<CustomerInfo>(entity);
    }

    public async Task<IEnumerable<CustomerInfo>> ValidDelete(String[] ids)
    {
        List<CustomerInfo> items = new List<CustomerInfo>();
        foreach (var id in ids)
        {
            var entity = await _repo.FindById(id);
            // MakeSure(entity != null, "NOT_FOUND");
            // var item = _mapper.Map<PayItemInfo>(entity);
            items.Add(_mapper.Map<CustomerInfo>(entity));
            // var payrollEnt = await _repoPayrollDetail.Query(f => f.NbpayitemId == id).FirstOrDefaultAsync();
            // MakeSure(payrollEnt == null, "DELETE_EXISTS", new { field0 = entity.Itemname, field1 = "form.BPR" });
            // var payslipEnt = await _repoPayslipDetail.Query(f => f.NbpayitemId == id).FirstOrDefaultAsync();
            // MakeSure(payslipEnt == null, "DELETE_EXISTS", new { field0 = entity.Itemname, field1 = "form.BPR" });

        }
        return items;
    }

    public async Task<bool> Delete(String id)
    {
        var entity = await _repo.FindByIdAndDelete(id);
        await _repo.SaveChanges();
        return true;
    }

    public async Task<bool> UserDelete(String id)
    {
        var entity = await _repo.Query(f => f.CustomerId == id).FirstOrDefaultAsync();
        if (entity != null) entity.Status = "H";
        await _repo.SaveChanges();
        return true;
    }
    public async Task<bool> Deletes(String[] ids)
    {
        var items = await ValidDelete(ids);
        foreach (var item in items)
        {
            await _repo.FindByIdAndDelete(item.CustomerId);
        }
        await _repo.SaveChanges();
        return true;
    }

    public async Task<bool> UserDeletes(String[] ids)
    {
        var items = await ValidDelete(ids);
        foreach (var item in items)
        {
            var entity = await _repo.Query(f => f.CustomerId == item.CustomerId).FirstOrDefaultAsync();
            if (entity != null) entity.Status = "H";
        }
        await _repo.SaveChanges();
        return true;
    }

    public async Task<IEnumerable<CustomerSummaryInfo>> GetSummary(QueryParamModel model)
    {
        var myApartmentCount = await QueryTotal(model);

        var data = new List<CustomerSummaryInfo>();
        data.Add(new CustomerSummaryInfo
        {
            Key = "MYCUSTOMER",
            Count = myApartmentCount
        });
        return _mapper.Map<IEnumerable<CustomerSummaryInfo>>(data);
    }

    public async Task<bool> AssignmenCustomer(AssignmentAaprtmentModel model)
    {
        var entity = await _repo.UpdateMany(f => model.Ids.Contains(f.CustomerId), s => s.SetProperty(f => f.UserId, model.Assignee));

        foreach (var id in model.Ids)
        {
            var assLogInf = new AssignmentLogInfo
            {
                FormId = "customer",
                ReferenceId = id,
                Assignee = model.Assignee,
                Accepting = "Y",
                Date = DateTime.UtcNow,
            };
            await _repoAssLog.Create(assLogInf);
        }


        await _repo.SaveChanges();
        return _mapper.Map<bool>(entity);
    }

    public async Task<IEnumerable<AssignmentLogInfo>> GetAssignment(string id)
    {
        var query = _repoAssLog.Query(f => f.FormId == "customer" && f.ReferenceId == id).Select(item => new AssignmentLog
        {
            Id = item.Id,
            FormId = item.FormId,
            ReferenceId = item.ReferenceId,
            Assignee = item.Assignee,
            User = item.User,
            Date = item.Date,

        });
        query = ApplySort(query, new SortModel[] { new SortModel { Property = "Date", Direction = "desc" } });

        var data = await query.ToListAsync();
        if (data.Count == 0)
        {
            data = _repo.Query(f => f.CustomerId == id).Select(f => new AssignmentLog
            {
                FormId = "customer",
                ReferenceId = f.CustomerId,
                Assignee = f.UserId,
                User = f.User,
                Date = f.LastUpdate
            }).ToList();
        }

        return _mapper.Map<IEnumerable<AssignmentLogInfo>>(data);
    }
    public async Task<TelephoneExistsInfo> IsTelephoneExists(string customerId, string[] phones)
    {
        var userId = GetUserId();
        var query = _repo.Query(f => f.Status != "H" && (phones.Contains(f.Telephone) || phones.Contains(f.Telephone2) || phones.Contains(f.Telephone3) || phones.Contains(f.Telephone4)))
        .Select(f => new Customer
        {
            UserId = f.UserId,
            Customername = f.Customername,
            Telephone = f.Telephone,
            Telephone2 = f.Telephone2,
            Telephone3 = f.Telephone3,
            Telephone4 = f.Telephone4,
            CustomerId = f.CustomerId,
            User = f.User
        });

        var data = await query.ToListAsync();

        if (string.IsNullOrEmpty(customerId))
        {
            if (data.Count > 0)
            {
                var items = data.FindAll(f => f.UserId != userId).Select(i => new ApartmentCheckTelephoneExistsInfo
                {
                    username = i.User != null ? i.User.Name : i.UserId,
                    customername = i.Customername
                }).ToArray();

                return new TelephoneExistsInfo { exists = true, items = items };
            }
            else return new TelephoneExistsInfo { exists = false, items = [] };
        }
        else
        {
            if (data.Find(f => f.CustomerId != customerId) == null) return new TelephoneExistsInfo { exists = false, items = [] };
            else
            {
                var items = data.FindAll(f => f.UserId != userId).Select(i => new ApartmentCheckTelephoneExistsInfo
                {
                    username = i.User != null ? i.User.Name : i.UserId,
                    customername = i.Customername
                }).ToArray();

                return new TelephoneExistsInfo { exists = true, items = items };
            }

        }
    }
}

