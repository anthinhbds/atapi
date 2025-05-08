

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

public interface IProjectService
{
    Task<ProjectInfo> Get(string id);
    Task<IEnumerable<ProjectInfo>> Query(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryTotal(QueryParamModel model);
    Task<IEnumerable<DistrictInfo>> QueryDistrict(QueryParamModel model);
    Task<IEnumerable<ProjectSummaryInfo>> GetSummary(QueryParamModel model);
    Task<ProjectInfo> Add(ProjectInfo obj);
    Task<ProjectInfo> Update(ProjectInfo obj);
    Task<bool> Archive(string[] ids);
    Task<bool> ArchiveAll();
    Task<bool> Restore(string[] ids);
    Task<bool> RestoreAll();
    Task<bool> Delete(String id);
    Task<bool> Deletes(String[] model);
    Task<bool> DeleteAll(DeleteAllInfo model);
    // Task<bool> CheckExistName(PayItemInfo obj);
}
public class ProjectService : GenericService, IProjectService
{
    protected readonly IMapper _mapper;
    readonly GeneralReposity<Project> _repo;
    readonly GeneralReposity<District> _repoDistrict;
    public ProjectService(IHttpContextAccessor http, AtDbContext db, IMapper mapper)
    : base(http, db)
    {
        _mapper = mapper;
        _repo = new BaseReposity<Project>(db, http);
        _repoDistrict = new BaseReposity<District>(db, http);
    }
    public async Task<ProjectInfo> Get(string id)
    {
        var query = _repo.Query(f => f.ProjectId == id);
        var data = await query.FirstOrDefaultAsync();
        return _mapper.Map<ProjectInfo>(data);
    }
    public async Task<IEnumerable<ProjectInfo>> Query(QueryParamModel model, int? page, int? pageSize)
    {

        var query = ApplyFilter(_repo, model, ["projectname", "district.districtname"]);

        query = query.Select(item => new Project
        {
            ProjectId = item.ProjectId,
            Projectname = item.Projectname,
            Status = item.Status,
            Arearange = item.Arearange,
            Owner = item.Owner,
            StreetId = item.StreetId,
            WardId = item.WardId,
            DistrictId = item.DistrictId,
            District = item.District
        });


        if (model.Sort == null)
        {
            query = ApplySort(query, new SortModel[] { new SortModel { Property = "projectname", Direction = "asc" } });
        }
        else
            query = ApplySort(query, model.Sort);


        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();

        return _mapper.Map<IEnumerable<ProjectInfo>>(data);
    }


    public async Task<int> QueryTotal(QueryParamModel model)
    {
        var qry = ApplyFilter(_repo, model, ["projectname", "district.districtname"]);
        var c = await qry.CountAsync();
        return c;
    }
    public async Task<ProjectInfo> Add(ProjectInfo obj)
    {

        var ids = await _repo.Query(f => true).Select(f => int.Parse(f.ProjectId)).ToListAsync();
        var maxProjectId = ids.Max();
        obj.ProjectId = (maxProjectId + 1).ToString();
        var entity = await _repo.Create(obj);
        // entity.CreatedBy

        await _repo.SaveChanges();
        return _mapper.Map<ProjectInfo>(entity);
    }

    public async Task<ProjectInfo> Update(ProjectInfo obj)
    {
        var entity = await _repo.FindByIdAndUpdate(obj, obj.ProjectId);



        await _repo.SaveChanges();
        return _mapper.Map<ProjectInfo>(entity);
    }

    public async Task<bool> Archive(String[] ids)
    {
        var idList = ids.ToList();
        var entity = await _repo.UpdateMany(item => idList.Contains(item.ProjectId), s => s
        .SetProperty(p => p.Archived, 1));
        await _repo.SaveChanges();
        return true;
    }
    public async Task<bool> ArchiveAll()
    {
        var entities = await _repo.FindAll(s => s.Archived == 0);
        foreach (var e in entities)
        {
            e.Archived = 1;
        }
        await _repo.SaveChanges();
        return true;
    }
    public async Task<bool> Restore(String[] ids)
    {
        var idList = ids.ToList();
        var entity = await _repo.UpdateMany(item => idList.Contains(item.ProjectId), s => s
        .SetProperty(p => p.Archived, 0));
        await _repo.SaveChanges();
        return true;
    }
    public async Task<bool> RestoreAll()
    {
        var entities = await _repo.FindAll(s => s.Archived == 1);
        foreach (var e in entities)
        {
            e.Archived = 0;
        }
        await _repo.SaveChanges();
        return true;
    }

    public async Task<IEnumerable<ProjectInfo>> ValidDelete(String[] ids)
    {
        List<ProjectInfo> items = new List<ProjectInfo>();
        foreach (var id in ids)
        {
            var entity = await _repo.FindById(id);
            // MakeSure(entity != null, "NOT_FOUND");
            // var item = _mapper.Map<PayItemInfo>(entity);
            items.Add(_mapper.Map<ProjectInfo>(entity));
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

    public async Task<bool> DeleteAll(DeleteAllInfo obj)
    {
        var ids = await _repo.Query(f => f.Archived == obj.Archived).Select(f => f.ProjectId).ToArrayAsync();
        return await Deletes(ids);
    }

    public async Task<bool> Deletes(String[] ids)
    {
        var items = await ValidDelete(ids);
        foreach (var item in items)
        {
            await _repo.FindByIdAndDelete(item.ProjectId);
        }
        await _repo.SaveChanges();
        return true;
    }

    public async Task<IEnumerable<ProjectSummaryInfo>> GetSummary(QueryParamModel model)
    {
        if (model.Filter == null)
        {
            model.Filter = new List<FilterModel>();
        }
        var archiveFilter = model.Filter.Where(f => f.Property != "archived");
        var predicateArchive = GetFilter<Project>(archiveFilter);

        var predicate = GetFilter<Project>(model.Filter);

        var searchString = model.searchString;

        if (!string.IsNullOrEmpty(searchString))
        {
            predicate = GetSearchContain<Project>(["projectId", "projectname"], searchString);
            predicateArchive = GetSearchContain<Project>(["projectId", "projectname"], searchString);
        }

        var query = _repo.Query(f => true).Where(predicate);


        var data = await query.GroupBy(x => x.Archived).Select(g => new ProjectSummaryInfo
        {
            Archived = g.Key,
            Count = g.Count()
        }).ToListAsync();

        return _mapper.Map<IEnumerable<ProjectSummaryInfo>>(data);
    }

    public async Task<IEnumerable<DistrictInfo>> QueryDistrict(QueryParamModel model)
    {

        var query = ApplyFilter(_repoDistrict, model, ["districtname"]);


        if (model.Sort == null)
        {
            query = ApplySort(query, new SortModel[] { new SortModel { Property = "districtname", Direction = "asc" } });
        }
        else
            query = ApplySort(query, model.Sort);


        var data = await query.ToListAsync();

        return _mapper.Map<IEnumerable<DistrictInfo>>(data);
    }
}

