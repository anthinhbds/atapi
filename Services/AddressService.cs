

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

public interface IAddressService
{
    Task<IEnumerable<DistrictInfo>> GetDistrict(QueryParamModel model, int? page, int? pageSize);
    Task<int> GetDistrictTotal(QueryParamModel model);
    Task<IEnumerable<WardInfo>> GetWard(QueryParamModel model, int? page, int? pageSize);
    Task<int> GetWardTotal(QueryParamModel model);
    // Task<IEnumerable<ProjectSummaryInfo>> GetSummary(QueryParamModel model);
    // Task<UserInfo> Add(UserInfo obj);
    // Task<UserInfo> Update(UserInfo obj);
    // Task<bool> Archive(Guid[] ids);
    // Task<bool> ArchiveAll();
    // Task<bool> Restore(Guid[] ids);
    // Task<bool> RestoreAll();
    // Task<bool> Delete(Guid nbpoId);
    // Task<bool> Deletes(Guid[] model);
    // Task<bool> DeleteAll(DeleteAllInfo model);
    // Task<bool> CheckExistName(PayItemInfo obj);
}
public class AddressService : GenericService, IAddressService
{
    protected readonly IMapper _mapper;
    readonly GeneralReposity<District> _repoDistrict;
    readonly GeneralReposity<Ward> _repoWard;
    public AddressService(IHttpContextAccessor http, AtDbContext db, IMapper mapper)
    : base(http, db)
    {
        _mapper = mapper;
        _repoDistrict = new BaseReposity<District>(db, http);
        _repoWard = new BaseReposity<Ward>(db, http);
    }

    public async Task<IEnumerable<DistrictInfo>> GetDistrict(QueryParamModel model, int? page, int? pageSize)
    {

        var query = ApplyFilter(_repoDistrict, model, ["districtname"], "districtId");

        if (model.Sort == null)
        {
            query = ApplySort(query, new SortModel[] { new SortModel { Property = "districtname", Direction = "asc" } });
        }
        else
            query = ApplySort(query, model.Sort);


        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();

        return _mapper.Map<IEnumerable<DistrictInfo>>(data);
    }


    public async Task<int> GetDistrictTotal(QueryParamModel model)
    {
        var qry = ApplyFilter(_repoDistrict, model, ["districtname"], "districtId");
        var c = await qry.CountAsync();
        return c;
    }

    public async Task<IEnumerable<WardInfo>> GetWard(QueryParamModel model, int? page, int? pageSize)
    {

        var query = ApplyFilter(_repoWard, model, ["wardname"], "wardId");

        if (model.Sort == null)
        {
            query = ApplySort(query, new SortModel[] { new SortModel { Property = "wardname", Direction = "asc" } });
        }
        else
            query = ApplySort(query, model.Sort);


        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();

        return _mapper.Map<IEnumerable<WardInfo>>(data);
    }


    public async Task<int> GetWardTotal(QueryParamModel model)
    {
        var qry = ApplyFilter(_repoWard, model, ["wardname"], "wardId");
        var c = await qry.CountAsync();
        return c;
    }
}

