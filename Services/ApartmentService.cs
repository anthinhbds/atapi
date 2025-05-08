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

public interface IApartmentService
{
    Task<ApartmentInfo> Get(string id);
    Task<IEnumerable<ApartmentNoteInfo>> GetNotes(string id);
    Task<IEnumerable<ApartmentInfo>> Query(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryTotal(QueryParamModel model);
    Task<IEnumerable<ApartmentInfo>> QueryPartner(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryPartnerTotal(QueryParamModel model);
    Task<IEnumerable<ApartmentInfo>> QueryAll(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryAllTotal(QueryParamModel model);
    Task<IEnumerable<ApartmentComboInfo>> QueryCombo(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryComboTotal(QueryParamModel model);
    Task<IEnumerable<ApartmentInfo>> QueryExpired(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryExpiredTotal(QueryParamModel model);
    Task<IEnumerable<ApartmentInfo>> QueryAssignment(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryAssignmentTotal(QueryParamModel model);
    Task<IEnumerable<ApartmentSummaryInfo>> GetSummary(QueryParamModel model);
    Task<ApartmentInfo> Add(ApartmentInfo obj);
    Task<ApartmentInfo> Update(ApartmentInfo obj);
    // Task<bool> Archive(string[] ids);
    // Task<bool> ArchiveAll();
    // Task<bool> Restore(string[] ids);
    // Task<bool> RestoreAll();
    Task<bool> Delete(String id);
    Task<bool> Deletes(String[] model);
    Task<bool> UserDelete(String id);
    Task<bool> UserDeletes(String[] model);
    Task<bool> AssignmentAaprtment(AssignmentAaprtmentModel model);
    Task<IEnumerable<AssignmentLogInfo>> GetAssignment(string id);
    Task<TelephoneExistsInfo> IsTelephoneExists(string apartmentId, string[] phones);
}
public class ApartmentService : GenericService, IApartmentService
{
    protected readonly IMapper _mapper;
    readonly GeneralReposity<Apartment> _repo;
    readonly GeneralReposity<ApartmentNote> _repoNote;
    readonly GeneralReposity<AssignmentLog> _repoAssLog;
    public ApartmentService(IHttpContextAccessor http, AtDbContext db, IMapper mapper)
    : base(http, db)
    {
        _mapper = mapper;
        _repo = new BaseReposity<Apartment>(db, http);
        _repoNote = new BaseReposity<ApartmentNote>(db, http);
        _repoAssLog = new BaseReposity<AssignmentLog>(db, http);
    }
    public async Task<ApartmentInfo> Get(string id)
    {
        var query = _repo.Query(f => f.ApartmentId == id);
        var data = await query.FirstOrDefaultAsync();
        return _mapper.Map<ApartmentInfo>(data);
    }
    public async Task<IEnumerable<ApartmentNoteInfo>> GetNotes(string id)
    {
        var query = _repoNote.Query(f => f.ApartmentId == id && f.Type == "N");
        var data = await query.ToListAsync();
        return _mapper.Map<IEnumerable<ApartmentNoteInfo>>(data);
    }
    public async Task<IEnumerable<ApartmentInfo>> Query(QueryParamModel model, int? page, int? pageSize)
    {
        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
            await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
            await HasPermit(EClaimId.Employee, EClaimValue.Leader);

        var today = DateTime.UtcNow;
        today = today.AddMonths(-3);
        var expiryDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Utc);
        string[] states = ["H"];

        var query = ApplyFilter(_repo, model, ["project.Projectname", "owner", "apartmentno", "lookupcode", "partnertelephone"]);
        query = query.Where(f => !states.Contains(f.Status) && f.LastUpdate > expiryDate && f.Ispartner == "N");

        if (!isAdmin) query = query.Where(f => f.UserId == GetUserId());

        query = query.Select(item => new Apartment
        {
            ApartmentId = item.ApartmentId,
            Demand = item.Demand,
            ProjectId = item.ProjectId,
            Project = item.Project,
            Salesprice = item.Salesprice,
            Rentprice = item.Rentprice,
            Salesfee = item.Salesfee,
            Rentfee = item.Rentfee,
            Area = item.Area,
            Bedroom = item.Bedroom,
            Apartmentview = item.Apartmentview,
            Status = item.Status,
            Apartmentno = item.Apartmentno,
            Owner = item.Owner,
            Telephone = item.Telephone,
            Telephone2 = item.Telephone2,
            Telephone3 = item.Telephone3,
            Telephone4 = item.Telephone4,
            Telephone5 = item.Telephone5,
            Lookupcode = item.Lookupcode,
            Furniture = item.Furniture,
            Banconyview = item.Banconyview,
            UserId = item.UserId,
            PrevioususerId = item.PrevioususerId,
            Notes = item.Notes,
            Priority = item.Priority,
            Ispartner = item.Ispartner,
            Partnername = item.Partnername,
            Partnertelephone = item.Partnertelephone,
            CreatedBy = item.CreatedBy,
            CreatedDate = item.CreatedDate,
            UpdatedBy = item.UpdatedBy,
            Sortdate = item.LastUpdate,
            LastUpdate = item.LastUpdate,
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

        return _mapper.Map<IEnumerable<ApartmentInfo>>(data);
    }

    public async Task<int> QueryTotal(QueryParamModel model)
    {
        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
           await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
           await HasPermit(EClaimId.Employee, EClaimValue.Leader);

        // if (isAdmin) model.Filter = model.Filter.Where(f => f.Property != "userId").ToList();

        var today = DateTime.UtcNow;
        today = today.AddMonths(-3);
        var expiryDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Utc);
        string[] states = ["H"];

        var qry = ApplyFilter(_repo, model, ["project.Projectname", "owner", "apartmentno", "lookupcode", "partnertelephone"]);
        qry = qry.Where(f => !states.Contains(f.Status) && f.LastUpdate > expiryDate && f.Ispartner == "N");

        if (!isAdmin) qry = qry.Where(f => f.UserId == GetUserId());

        var c = await qry.CountAsync();
        return c;
    }

    public async Task<IEnumerable<ApartmentInfo>> QueryAll(QueryParamModel model, int? page, int? pageSize)
    {
        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
            await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
            await HasPermit(EClaimId.Employee, EClaimValue.Leader);

        string[] states = ["H"];
        var query = ApplyFilter(_repo, model, ["project.Projectname", "owner", "apartmentno", "lookupcode", "partnertelephone"]);
        query = query.Where(f => !states.Contains(f.Status));

        query = query.Select(item => new Apartment
        {
            ApartmentId = item.ApartmentId,
            Demand = item.Demand,
            ProjectId = item.ProjectId,
            Project = item.Project,
            Salesprice = item.Salesprice,
            Rentprice = item.Rentprice,
            Salesfee = item.Salesfee,
            Rentfee = item.Rentfee,
            Area = item.Area,
            Bedroom = item.Bedroom,
            Apartmentview = item.Apartmentview,
            Status = item.Status,
            Apartmentno = item.Apartmentno,
            Owner = item.Owner,
            Telephone = isAdmin ? item.Telephone : item.Telephone.Substring(item.Telephone.Length - 5),
            Telephone2 = isAdmin ? item.Telephone2 : item.Telephone2.Substring(item.Telephone2.Length - 5),
            Telephone3 = isAdmin ? item.Telephone3 : item.Telephone3.Substring(item.Telephone3.Length - 5),
            Telephone4 = isAdmin ? item.Telephone4 : item.Telephone4.Substring(item.Telephone4.Length - 5),
            Telephone5 = isAdmin ? item.Telephone5 : item.Telephone5.Substring(item.Telephone5.Length - 5),
            Lookupcode = item.Lookupcode,
            Furniture = item.Furniture,
            Banconyview = item.Banconyview,
            UserId = item.UserId,
            PrevioususerId = item.PrevioususerId,
            Notes = item.Notes,
            Priority = item.Priority,
            Ispartner = item.Ispartner,
            Partnername = item.Partnername,
            Partnertelephone = item.Partnertelephone,
            CreatedBy = item.CreatedBy,
            CreatedDate = item.CreatedDate,
            UpdatedBy = item.UpdatedBy,
            LastUpdate = item.LastUpdate,
            Sortdate = item.LastUpdate,
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

        return _mapper.Map<IEnumerable<ApartmentInfo>>(data);
    }
    public async Task<int> QueryAllTotal(QueryParamModel model)
    {
        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
           await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
           await HasPermit(EClaimId.Employee, EClaimValue.Leader);

        string[] states = ["H"];

        var qry = ApplyFilter(_repo, model, ["project.Projectname", "owner", "apartmentno", "lookupcode", "partnertelephone"]);
        qry = qry.Where(f => !states.Contains(f.Status));

        var c = await qry.CountAsync();
        return c;
    }

    public async Task<IEnumerable<ApartmentInfo>> QueryPartner(QueryParamModel model, int? page, int? pageSize)
    {
        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
            await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
            await HasPermit(EClaimId.Employee, EClaimValue.Leader);

        var today = DateTime.UtcNow;
        today = today.AddMonths(-3);
        var expiryDate = new DateTime(today.Year, today.Month, Constants.expiryDay, 0, 0, 0, DateTimeKind.Utc);
        string[] states = ["H"];

        var query = ApplyFilter(_repo, model, ["project.Projectname", "owner", "apartmentno", "lookupcode", "partnertelephone"]);
        query = query.Where(f => !states.Contains(f.Status) && f.LastUpdate >= expiryDate && f.Ispartner == "Y");

        if (!isAdmin) query = query.Where(f => f.UserId == GetUserId());

        query = query.Select(item => new Apartment
        {
            ApartmentId = item.ApartmentId,
            Demand = item.Demand,
            ProjectId = item.ProjectId,
            Project = item.Project,
            Salesprice = item.Salesprice,
            Rentprice = item.Rentprice,
            Salesfee = item.Salesfee,
            Rentfee = item.Rentfee,
            Area = item.Area,
            Bedroom = item.Bedroom,
            Apartmentview = item.Apartmentview,
            Status = item.Status,
            Apartmentno = item.Apartmentno,
            Owner = item.Owner,
            // Telephone = isAdmin ? item.Telephone : item.Telephone.Substring(item.Telephone.Length - 5),
            // Telephone2 = isAdmin ? item.Telephone2 : item.Telephone2.Substring(item.Telephone2.Length - -5),
            // Telephone3 = isAdmin ? item.Telephone3 : item.Telephone3.Substring(item.Telephone3.Length - -5),
            // Telephone4 = isAdmin ? item.Telephone4 : item.Telephone4.Substring(item.Telephone4.Length - -5),
            // Telephone5 = isAdmin ? item.Telephone5 : item.Telephone5.Substring(item.Telephone5.Length - -5),
            Telephone = item.Telephone,
            Telephone2 = item.Telephone2,
            Telephone3 = item.Telephone3,
            Telephone4 = item.Telephone4,
            Telephone5 = item.Telephone5,
            Lookupcode = item.Lookupcode,
            Furniture = item.Furniture,
            Banconyview = item.Banconyview,
            UserId = item.UserId,
            PrevioususerId = item.PrevioususerId,
            Notes = item.Notes,
            Priority = item.Priority,
            Ispartner = item.Ispartner,
            Partnername = item.Partnername,
            Partnertelephone = item.Partnertelephone,
            CreatedBy = item.CreatedBy,
            CreatedDate = item.CreatedDate,
            UpdatedBy = item.UpdatedBy,
            LastUpdate = item.LastUpdate,
            Sortdate = item.LastUpdate,
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

        return _mapper.Map<IEnumerable<ApartmentInfo>>(data);
    }

    public async Task<int> QueryPartnerTotal(QueryParamModel model)
    {
        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
           await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
           await HasPermit(EClaimId.Employee, EClaimValue.Leader);

        // if (isAdmin) model.Filter = model.Filter.Where(f => f.Property != "userId").ToList();

        var today = DateTime.UtcNow;
        today = today.AddMonths(-3);
        var expiryDate = new DateTime(today.Year, today.Month, Constants.expiryDay, 0, 0, 0, DateTimeKind.Utc);
        string[] states = ["H"];

        var qry = ApplyFilter(_repo, model, ["project.Projectname", "owner", "apartmentno", "lookupcode", "partnertelephone"]);
        qry = qry.Where(f => !states.Contains(f.Status) && f.LastUpdate >= expiryDate && f.Ispartner == "Y");

        if (!isAdmin) qry = qry.Where(f => f.UserId == GetUserId());

        var c = await qry.CountAsync();
        return c;
    }

    public async Task<IEnumerable<ApartmentInfo>> QueryExpired(QueryParamModel model, int? page, int? pageSize)
    {
        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
            await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
            await HasPermit(EClaimId.Employee, EClaimValue.Leader);

        var today = DateTime.UtcNow;
        today = today.AddMonths(-3);
        var expiryDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Utc);
        string[] states = [EApartmentStatus.H];

        var query = ApplyFilter(_repo, model, ["project.Projectname", "owner", "apartmentno", "telephone", "telephone2", "telephone3"]);
        query = query.Where(f => !states.Contains(f.Status) && f.LastUpdate < expiryDate);

        if (!isAdmin) query = query.Where(f => f.UserId == GetUserId());

        query = query.Select(item => new Apartment
        {
            ApartmentId = item.ApartmentId,
            Demand = item.Demand,
            ProjectId = item.ProjectId,
            Project = item.Project,
            Salesprice = item.Salesprice,
            Rentprice = item.Rentprice,
            Salesfee = item.Salesfee,
            Rentfee = item.Rentfee,
            Area = item.Area,
            Bedroom = item.Bedroom,
            Apartmentview = item.Apartmentview,
            Status = item.Status,
            Apartmentno = item.Apartmentno,
            Owner = item.Owner,
            Telephone = item.Telephone,
            Telephone2 = item.Telephone2,
            Telephone3 = item.Telephone3,
            Telephone4 = item.Telephone4,
            Telephone5 = item.Telephone5,
            Lookupcode = item.Lookupcode,
            Furniture = item.Furniture,
            Banconyview = item.Banconyview,
            UserId = item.UserId,
            PrevioususerId = item.PrevioususerId,
            Notes = item.Notes,
            Priority = item.Priority,
            Ispartner = item.Ispartner,
            Partnername = item.Partnername,
            Partnertelephone = item.Partnertelephone,
            CreatedBy = item.CreatedBy,
            CreatedDate = item.CreatedDate,
            UpdatedBy = item.UpdatedBy,
            LastUpdate = item.LastUpdate,
            Sortdate = item.LastUpdate,
        });


        if (model.Sort == null)
        {
            query = ApplySort(query, new SortModel[] { new SortModel { Property = "Priority", Direction = "desc" } });
        }
        else
            query = ApplySort(query, model.Sort);

        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();

        return _mapper.Map<IEnumerable<ApartmentInfo>>(data);
    }


    public async Task<int> QueryExpiredTotal(QueryParamModel model)
    {
        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
            await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
            await HasPermit(EClaimId.Employee, EClaimValue.Leader);

        var today = DateTime.UtcNow;
        today = today.AddMonths(-3);
        var expiryDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Utc);
        string[] states = [EApartmentStatus.H];

        var qry = ApplyFilter(_repo, model, ["project.Projectname", "owner", "apartmentno", "telephone", "telephone2", "telephone3"]);
        qry = qry.Where(f => !states.Contains(f.Status) && f.LastUpdate < expiryDate);
        if (!isAdmin) qry = qry.Where(f => f.UserId == GetUserId());
        var c = await qry.CountAsync();
        return c;
    }

    public async Task<IEnumerable<ApartmentInfo>> QueryAssignment(QueryParamModel model, int? page, int? pageSize)
    {
        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
            await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
            await HasPermit(EClaimId.Employee, EClaimValue.Leader);

        string[] states = ["H"];

        var query = ApplyFilter(_repo, model, ["project.Projectname", "owner", "apartmentno", "lookupcode", "partnertelephone"]);
        query = query.Where(f => !states.Contains(f.Status) && f.Ispartner == "N");

        if (!isAdmin) query = query.Where(f => f.UserId == GetUserId());

        query = query.Join(_db.AssignmentLogs, a => a.ApartmentId, l => l.ReferenceId, (a, l) => new { a, l })
        .Where(f => f.l.FormId == "apartment" && f.l.Assignee == GetUserId() && f.l.Accepting == "Y")
        .Select(item => new Apartment
        {
            ApartmentId = item.a.ApartmentId,
            Demand = item.a.Demand,
            ProjectId = item.a.ProjectId,
            Project = item.a.Project,
            Salesprice = item.a.Salesprice,
            Rentprice = item.a.Rentprice,
            Salesfee = item.a.Salesfee,
            Rentfee = item.a.Rentfee,
            Area = item.a.Area,
            Bedroom = item.a.Bedroom,
            Apartmentview = item.a.Apartmentview,
            Status = item.a.Status,
            Apartmentno = item.a.Apartmentno,
            Owner = item.a.Owner,
            Telephone = item.a.Telephone,
            Telephone2 = item.a.Telephone2,
            Telephone3 = item.a.Telephone3,
            Telephone4 = item.a.Telephone4,
            Telephone5 = item.a.Telephone5,
            Lookupcode = item.a.Lookupcode,
            Furniture = item.a.Furniture,
            Banconyview = item.a.Banconyview,
            UserId = item.a.UserId,
            PrevioususerId = item.a.PrevioususerId,
            Notes = item.a.Notes,
            Priority = item.a.Priority,
            CreatedBy = item.a.CreatedBy,
            CreatedDate = item.a.CreatedDate,
            UpdatedBy = item.a.UpdatedBy,
            Sortdate = item.l.Date,
            LastUpdate = item.l.Date,
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

        return _mapper.Map<IEnumerable<ApartmentInfo>>(data);
    }

    public async Task<int> QueryAssignmentTotal(QueryParamModel model)
    {
        var isAdmin = await HasPermit(EClaimId.Admin, EClaimValue.Standard) ||
           await HasPermit(EClaimId.Admin, EClaimValue.ManagerUser) ||
           await HasPermit(EClaimId.Employee, EClaimValue.Leader);


        string[] states = ["H"];

        var qry = ApplyFilter(_repo, model, ["project.Projectname", "owner", "apartmentno", "lookupcode", "partnertelephone"]);
        qry = qry.Where(f => !states.Contains(f.Status) && f.Ispartner == "N");

        if (!isAdmin) qry = qry.Where(f => f.UserId == GetUserId());

        qry = qry.Join(_db.AssignmentLogs, a => a.ApartmentId, l => l.ReferenceId, (a, l) => new { a, l })
        .Where(f => f.l.FormId == "apartment" && f.l.Assignee == GetUserId() && f.l.Accepting == "Y")
        .Select(item => item.a);

        var c = await qry.CountAsync();
        return c;
    }

    public async Task<ApartmentInfo> Add(ApartmentInfo obj)
    {
        var maxNo = await _repo.Query(b => b.ApartmentId.StartsWith("P") && Regex.IsMatch(b.ApartmentId.Substring(1), $@"^\d{{{9}}}$"))
            .MaxAsync(b => b.ApartmentId);

        if (maxNo == null) maxNo = "P" + 1.ToString($"D{9}");
        else
        {
            int max = int.Parse(maxNo.Substring(maxNo.Length - 9)) + 1;
            maxNo = "P" + max.ToString($"D{9}");
        }
        var entity = await _repo.Create(obj);
        entity.ApartmentId = maxNo;

        if (obj.Details != null)
        {
            foreach (var d in obj.Details)
            {
                d.ApartmentId = entity.ApartmentId;
                var entityDetail = await _repoNote.Create(d);
            }
        }

        await _repo.SaveChanges();
        return _mapper.Map<ApartmentInfo>(entity);
    }

    public async Task<ApartmentInfo> Update(ApartmentInfo obj)
    {
        var entity = await _repo.FindByIdAndUpdate(obj, obj.ApartmentId);

        if (obj.Details != null)
        {
            foreach (var d in obj.Details)
            {
                var entDetail = await _repoNote.FindOneAndUpdate(f => f.ApartmentId == entity.ApartmentId && f.Linenum == d.Linenum, d);
                if (entDetail == null)
                {
                    d.ApartmentId = entity.ApartmentId;
                    var entityDetail = await _repoNote.Create(d);

                }
            }
        }
        if (obj.D_Details != null)
        {
            foreach (var itemDelete in obj.D_Details)
            {
                await _repoNote.FindOneAndDelete(f => f.ApartmentId == entity.ApartmentId && f.Linenum == itemDelete.Linenum);
            }
        }
        var assignLogs = await _db.AssignmentLogs.Where(f => f.Assignee == entity.UserId &&
        f.ReferenceId == entity.ApartmentId &&
        f.Accepting == "Y" &&
        f.FormId == "apartment")
         .ToListAsync();

        if (assignLogs.Count > 0)
        {
            await _repoAssLog.UpdateMany(
                f => assignLogs.Select(f => f.ReferenceId).ToArray().Contains(f.ReferenceId),
                s => s.SetProperty(f => f.Accepting, "N"));
        }

        await _repo.SaveChanges();
        return _mapper.Map<ApartmentInfo>(entity);
    }

    public async Task<IEnumerable<ApartmentInfo>> ValidDelete(String[] ids)
    {
        List<ApartmentInfo> items = new List<ApartmentInfo>();
        foreach (var id in ids)
        {
            var entity = await _repo.FindById(id);
            // MakeSure(entity != null, "NOT_FOUND");
            // var item = _mapper.Map<PayItemInfo>(entity);
            items.Add(_mapper.Map<ApartmentInfo>(entity));
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
        var entity = await _repo.Query(f => f.ApartmentId == id).FirstOrDefaultAsync();
        if (entity != null)
        {
            entity.Status = EApartmentStatus.H;
        }
        await _repo.SaveChanges();
        return true;
    }
    public async Task<bool> Deletes(String[] ids)
    {
        var items = await ValidDelete(ids);
        foreach (var item in items)
        {
            await _repo.FindByIdAndDelete(item.ApartmentId);
        }
        await _repo.SaveChanges();
        return true;
    }

    public async Task<bool> UserDeletes(String[] ids)
    {
        var items = await ValidDelete(ids);
        foreach (var item in items)
        {
            var entity = await _repo.Query(f => f.ApartmentId == item.ApartmentId).FirstOrDefaultAsync();
            if (entity != null)
            {
                entity.Status = EApartmentStatus.H;
                entity.LastUpdate = DateTime.UtcNow;
            }
        }
        await _repo.SaveChanges();
        return true;
    }

    public async Task<IEnumerable<ApartmentSummaryInfo>> GetSummary(QueryParamModel model)
    {
        var myApartmentCount = await QueryTotal(model);
        var partnerCount = await QueryPartnerTotal(model);
        var expiredCount = await QueryExpiredTotal(model);
        var assignmentCount = await QueryAssignmentTotal(model);

        var data = new List<ApartmentSummaryInfo>();
        data.Add(new ApartmentSummaryInfo
        {
            Key = "MYAPARTMENT",
            Count = myApartmentCount
        });
        data.Add(new ApartmentSummaryInfo
        {
            Key = "PARTNER",
            Count = partnerCount
        });
        data.Add(new ApartmentSummaryInfo
        {
            Key = "EXPIRED",
            Count = expiredCount
        });
        data.Add(new ApartmentSummaryInfo
        {
            Key = "ASSIGNMENT",
            Count = assignmentCount
        });

        return _mapper.Map<IEnumerable<ApartmentSummaryInfo>>(data);
    }

    public async Task<bool> AssignmentAaprtment(AssignmentAaprtmentModel model)
    {
        var entity = await _repo.UpdateMany(
            f => model.Ids.Contains(f.ApartmentId),
            s => s.SetProperty(f => f.UserId, model.Assignee).SetProperty(f => f.Priority, "N"));

        foreach (var id in model.Ids)
        {
            var assLogInf = new AssignmentLogInfo
            {
                FormId = "apartment",
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
        var query = _repoAssLog.Query(f => f.FormId == "apartment" && f.ReferenceId == id).Select(item => new AssignmentLog
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
            data = _repo.Query(f => f.ApartmentId == id).Select(f => new AssignmentLog
            {
                FormId = "apartment",
                ReferenceId = f.ApartmentId,
                Assignee = f.UserId,
                User = f.User,
                Date = f.LastUpdate
            }).ToList();
        }

        return _mapper.Map<IEnumerable<AssignmentLogInfo>>(data);
    }
    public async Task<TelephoneExistsInfo> IsTelephoneExists(string apartmentId, string[] phones)
    {
        var userId = GetUserId();

        var query = _repo.Query(f => f.Status != EApartmentStatus.H
         && (phones.Contains(f.Telephone) || phones.Contains(f.Telephone2) || phones.Contains(f.Telephone3) || phones.Contains(f.Telephone4) || phones.Contains(f.Telephone5)))
         .Select(f => new Apartment
         {
             ApartmentId = f.ApartmentId,
             UserId = f.UserId,
             User = f.User,
             Apartmentno = f.Apartmentno,
             Owner = f.Owner,
             Project = f.Project,
             Telephone = f.Telephone,
             Telephone2 = f.Telephone2,
             Telephone3 = f.Telephone3,
             Telephone4 = f.Telephone4,
             Telephone5 = f.Telephone5,
             Status = f.Status
         });

        var data = await query.ToListAsync();

        if (string.IsNullOrEmpty(apartmentId))
        {
            if (data.Count > 0)
            {
                var items = data.Select(i => new ApartmentCheckTelephoneExistsInfo
                {
                    userid = i.UserId,
                    username = i.User != null ? i.User.Name : i.UserId,
                    apartmentno = i.Apartmentno,
                    owner = i.Owner,
                    projectname = i.Project != null ? i.Project.Projectname : ""
                }).ToArray();

                return new TelephoneExistsInfo { exists = true, items = items };
            }
            else return new TelephoneExistsInfo { exists = false, items = [] };
        }
        else
        {
            if (data.Find(f => f.ApartmentId != apartmentId) == null) return new TelephoneExistsInfo { exists = false, items = [] };
            else
            {
                var items = data.Select(i => new ApartmentCheckTelephoneExistsInfo
                {
                    userid = i.UserId,
                    username = i.User != null ? i.User.Name : i.UserId,
                    apartmentno = i.Apartmentno,
                    owner = i.Owner,
                    projectname = i.Project != null ? i.Project.Projectname : ""
                }).ToArray();

                return new TelephoneExistsInfo { exists = true, items = items };
            }

        }
    }

    public async Task<IEnumerable<ApartmentComboInfo>> QueryCombo(QueryParamModel model, int? page, int? pageSize)
    {
        var displayapartment = new FilterModel();
        if (model.Filter != null)
        {
            displayapartment = model.Filter?.FirstOrDefault(f => f.Property == "displayapartment");
            model.Filter = model.Filter.Where(f => f.Property != "displayapartment");
        }

        var hasTextFilter = model.Filter != null ? model.Filter.Where(f => f.Property == "text").FirstOrDefault() : null;

        if (hasTextFilter != null) model.searchString = hasTextFilter.Value.ToString();

        var query = ApplyFilter(_repo, model, ["project.Projectname", "apartmentno", "owner"]);

        if (displayapartment != null && displayapartment.Property == "displayapartment")
        {

            string v = displayapartment.Value.ToString();
            // query = query.Where(f => f.Accountcode.Contains(v) || f.Accountname.Contains(v));
            query = query.Where(f => (f.Project.Projectname + " - " + f.Apartmentno + " - " + f.Owner + " - " + f.Telephone).ToUpper().Contains(v.ToUpper()));
        }

        query = query.Select(item => new Apartment
        {
            ApartmentId = item.ApartmentId,
            ProjectId = item.ProjectId,
            Project = item.Project,
            Apartmentno = item.Apartmentno,
            Owner = item.Owner,
            Displayapartment = item.Project.Projectname + " - " + item.Apartmentno + " - " + item.Owner + " - " + item.Telephone,
        });

        if (model.Sort != null)
            query = ApplySort(query, model.Sort);


        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();

        var obj = _mapper.Map<IEnumerable<ApartmentComboInfo>>(data);

        return obj;
    }

    public async Task<int> QueryComboTotal(QueryParamModel model)
    {
        var qry = ApplyFilter(_repo, model, ["project.Projectname", "apartmentno", "owner"]);
        var c = await qry.CountAsync();
        return c;
    }
}

