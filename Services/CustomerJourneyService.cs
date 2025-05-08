

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

public interface ICustomerJourneyService
{
    Task<CustomerJourneyInfo> Get(string id);
    Task<IEnumerable<CustomerJourneyDetInfo>> GetDetails(string customerId);
    Task<IEnumerable<CustomerJourneyInfo>> Query(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryTotal(QueryParamModel model);
    Task<CustomerJourneyInfo> Add(CustomerJourneyInfo obj);
    Task<CustomerJourneyInfo> Update(CustomerJourneyInfo obj);
    Task<bool> Delete(String id);
    Task<bool> Deletes(String[] model);
    Task<bool> DeleteAll(DeleteAllInfo model);
    Task<IEnumerable<CustomerInfo>> QueryCustomer(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryCustomerTotal(QueryParamModel model);
}
public class CustomerJourneyService : GenericService, ICustomerJourneyService
{
    protected readonly IMapper _mapper;
    readonly GeneralReposity<CustomerJourney> _repo;
    readonly GeneralReposity<CustomerJourneyDet> _repoDet;
    readonly GeneralReposity<Customer> _repoCus;
    public CustomerJourneyService(IHttpContextAccessor http, AtDbContext db, IMapper mapper)
    : base(http, db)
    {
        _mapper = mapper;
        _repo = new BaseReposity<CustomerJourney>(db, http);
        _repoDet = new BaseReposity<CustomerJourneyDet>(db, http);
        _repoCus = new BaseReposity<Customer>(db, http);
    }
    public async Task<CustomerJourneyInfo> Get(string id)
    {
        var query = _repo.Query(f => f.CustomerId == id);
        var data = await query.FirstOrDefaultAsync();
        return _mapper.Map<CustomerJourneyInfo>(data);
    }
    public async Task<IEnumerable<CustomerJourneyInfo>> Query(QueryParamModel model, int? page, int? pageSize)
    {
        var query = ApplyFilter(_repo, model, ["customer.telephone", "customer.customername"]);

        query = query.Select(item => new CustomerJourney
        {
            CustomerId = item.CustomerId,
            UserId = item.UserId,
            Customer = item.Customer,
            Comments = item.Comments,
            Status = item.Status,
            Demand = item.Demand,
            Finance = item.Finance,
            Searching = item.Searching,
            Quality = item.Quality,
            CreatedDate = item.CreatedDate,
            CreatedBy = item.CreatedBy,
            LastUpdate = item.LastUpdate,
            UpdatedBy = item.UpdatedBy,
        });


        if (model.Sort != null)
            query = ApplySort(query, model.Sort);
        else
            query = query.OrderBy(f => f.Quality).ThenByDescending(f => f.CreatedDate);


        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();

        return _mapper.Map<IEnumerable<CustomerJourneyInfo>>(data);
    }


    public async Task<int> QueryTotal(QueryParamModel model)
    {
        var qry = ApplyFilter(_repo, model, ["projectname", "district.districtname"]);
        var c = await qry.CountAsync();
        return c;
    }
    public async Task<IEnumerable<CustomerJourneyDetInfo>> GetDetails(string customerId)
    {
        var query = _repoDet.Query(f => f.CustomerId == customerId).OrderByDescending(f => f.Journeydate);
        var data = await query.ToListAsync();
        if (data != null)
        {
            foreach (var item in data)
            {
                var arr = item.Project.Split(';');
                var projects = await _db.Projects.Where(f => arr.Contains(f.ProjectId)).Select(f => f.Projectname).ToArrayAsync();
                string projectname = string.Join(", ", projects);
                item.Projectname = projectname;
            }
        }
        return _mapper.Map<IEnumerable<CustomerJourneyDetInfo>>(data);
    }


    public async Task<CustomerJourneyInfo> Add(CustomerJourneyInfo obj)
    {

        var entity = await _repo.Create(obj);
        // entity.CreatedBy
        if (obj.Details != null)
        {
            foreach (var d in obj.Details)
            {
                d.CustomerId = entity.CustomerId;
                d.Journeydate = DateTime.SpecifyKind(d.Journeydate.Value, DateTimeKind.Utc);
                var entityDetail = await _repoDet.Create(d);
            }
        }
        await _repo.SaveChanges();
        return _mapper.Map<CustomerJourneyInfo>(entity);
    }

    public async Task<CustomerJourneyInfo> Update(CustomerJourneyInfo obj)
    {

        var entity = await _repo.FindByIdAndUpdate(obj, obj.CustomerId);
        await _repoDet.DeleteMany(f => f.CustomerId == obj.CustomerId);
        // entity.CreatedBy
        if (obj.Details != null)
        {
            foreach (var d in obj.Details)
            {
                d.CustomerId = entity.CustomerId;
                d.Journeydate = DateTime.SpecifyKind(d.Journeydate.Value, DateTimeKind.Utc);
                var entityDetail = await _repoDet.Create(d);
            }
        }

        await _repo.SaveChanges();
        return _mapper.Map<CustomerJourneyInfo>(entity);
    }


    public async Task<IEnumerable<CustomerJourneyInfo>> ValidDelete(String[] ids)
    {
        List<CustomerJourneyInfo> items = new List<CustomerJourneyInfo>();
        foreach (var id in ids)
        {
            var entity = await _repo.FindById(id);
            // MakeSure(entity != null, "NOT_FOUND");
            // var item = _mapper.Map<PayItemInfo>(entity);
            items.Add(_mapper.Map<CustomerJourneyInfo>(entity));
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
        await _repoDet.DeleteMany(f => f.CustomerId == id);
        await _repo.SaveChanges();
        return true;
    }

    public async Task<bool> DeleteAll(DeleteAllInfo obj)
    {
        var ids = await _repo.Query(f => true).Select(f => f.CustomerId).ToArrayAsync();
        return await Deletes(ids);
    }

    public async Task<bool> Deletes(String[] ids)
    {
        var items = await ValidDelete(ids);
        foreach (var item in items)
        {
            await _repo.FindByIdAndDelete(item.CustomerId);
            await _repoDet.DeleteMany(f => f.CustomerId == item.CustomerId);
        }
        await _repo.SaveChanges();
        return true;
    }

    public async Task<IEnumerable<CustomerInfo>> QueryCustomer(QueryParamModel model, int? page, int? pageSize)
    {
        var displaycustomer = new FilterModel();
        if (model.Filter != null)
        {
            displaycustomer = model.Filter?.FirstOrDefault(f => f.Property == "displaycustomer");
            model.Filter = model.Filter.Where(f => f.Property != "displaycustomer");
        }


        var query = ApplyFilter(_repoCus, model, ["customername", "telephone", "telephone2", "telephone3", "notes"]);

        if (displaycustomer != null && displaycustomer.Property == "displaycustomer")
        {

            string v = displaycustomer.Value.ToString();
            // query = query.Where(f => f.Accountcode.Contains(v) || f.Accountname.Contains(v));
            query = query.Where(f => (f.Customername + " - " + f.Telephone).ToUpper().Contains(v.ToUpper()));
        }


        query = query.Select(item => new Customer
        {
            CustomerId = item.CustomerId,
            UserId = item.UserId,
            Displaycustomer = item.Customername + " - " + item.Telephone,
            Notes = item.Notes,
        });


        if (model.Sort != null)
            query = ApplySort(query, model.Sort);


        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();

        return _mapper.Map<IEnumerable<CustomerInfo>>(data);
    }

    public async Task<int> QueryCustomerTotal(QueryParamModel model)
    {
        var query = ApplyFilter(_repoCus, model, ["customername", "telephone", "telephone2", "telephone3", "notes"]);
        // var customerjourneys = await _repo.Query(f => true).Select(f => f.CustomerId).ToArrayAsync();
        // query = query.Where(f => !customerjourneys.Contains(f.CustomerId));
        var c = await query.CountAsync();
        return c;
    }
}

