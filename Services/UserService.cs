

using Microsoft.EntityFrameworkCore;
using MapsterMapper;
using System.Security.Claims;
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

public interface IUserService
{
    Task<UserInfo> Get(String id);
    Task<UserResultInfo> GetMe(string refreshToken);
    Task<IEnumerable<UserSearchProfileInfo>> GetSearchProfile(string formid);
    Task<IEnumerable<UserListInfo>> GetListUser(QueryParamModel model, int? page, int? pageSize);
    Task<IEnumerable<UserInfo>> Query(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryTotal(QueryParamModel model);
    Task<IEnumerable<UserSummaryInfo>> GetSummary(QueryParamModel model);
    Task<UserInfo> Add(UserInfo obj);
    Task<UserInfo> Update(UserInfo obj);
    Task<UserSearchProfileInfo> AddSearchProfile(UserSearchProfileInfo obj);
    Task<UserSearchProfileInfo> UpdateSearchProfile(UserSearchProfileInfo obj);
    Task<bool> DeleteSearchProfile(Guid id);
    Task<bool> CheckExistSearchProfile(UserSearchProfileInfo obj);
    Task<IEnumerable<UserComboInfo>> QueryCombo(QueryParamModel model, int? page, int? pageSize);
    Task<int> QueryComboTotal(QueryParamModel model);
}
public class UserService : GenericService, IUserService
{
    protected readonly IMapper _mapper;
    readonly BaseReposity<User> _repo;
    readonly GeneralReposity<UserClaim> _repoClaim;
    readonly GeneralReposity<UserSearchProfile> _repoSearch;
    public UserService(IHttpContextAccessor http, AtDbContext db, IMapper mapper)
    : base(http, db)
    {
        _mapper = mapper;
        _repo = new BaseReposity<User>(db, http);
        _repoClaim = new BaseReposity<UserClaim>(db, http);
        _repoSearch = new BaseReposity<UserSearchProfile>(db, http);
    }
    public async Task<UserInfo> Get(String id)
    {
        var query = _repo.Query(f => f.UserId == id).Select(item => new User
        {
            UserId = item.UserId,
            Name = item.Name,
            Telephone = item.Telephone,
            Email = item.Email,
        });
        var data = await query.FirstOrDefaultAsync();
        return _mapper.Map<UserInfo>(data);
    }
    public async Task<IEnumerable<UserInfo>> Query(QueryParamModel model, int? page, int? pageSize)
    {

        var qry = ApplyFilter(_repo, model, ["userid", "name"], "UserId");

        var query = qry.Select(item => new User
        {
            UserId = item.UserId,
            Name = item.Name,
            Telephone = item.Telephone,
            Email = item.Email,
            Claims = item.Claims
        });


        if (model.Sort == null)
        {
            query = ApplySort(query, new SortModel[] { new SortModel { Property = "name", Direction = "asc" } });
        }
        else
            query = ApplySort(query, model.Sort);


        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();

        return _mapper.Map<IEnumerable<UserInfo>>(data);
    }

    public async Task<int> QueryTotal(QueryParamModel model)
    {
        var qry = ApplyFilter(_repo, model, ["userid", "name"], "UserId");
        var c = await qry.CountAsync();
        return c;
    }

    public async Task<IEnumerable<UserListInfo>> GetListUser(QueryParamModel model, int? page, int? pageSize)
    {

        var qry = ApplyFilter(_repo, model, ["userid", "name"], "UserId");

        var query = qry.Select(item => new User
        {
            UserId = item.UserId,
            Name = item.Name
        });


        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();

        return _mapper.Map<IEnumerable<UserListInfo>>(data);
    }

    public async Task<UserInfo> Add(UserInfo obj)
    {
        var entity = await _repo.Create(obj);
        if (obj.Claims != null)
        {
            foreach (var claim in obj.Claims)
            {
                var claimEnt = new UserClaim
                {
                    UserId = obj.UserId,
                    ClaimId = claim.ClaimId,
                    Claimvalue = claim.Claimvalue
                };
                _db.UserClaims.Add(claimEnt);
            }
        }
        await _repo.SaveChanges();
        return _mapper.Map<UserInfo>(entity);
    }

    public async Task<UserInfo> Update(UserInfo obj)
    {
        var entity = await _repo.FindByIdAndUpdate(obj, obj.UserId);

        if (obj.Claims != null)
        {
            var claimEnts = await _db.UserClaims.Where(f => f.UserId == obj.UserId).ToListAsync();
            _db.UserClaims.RemoveRange(claimEnts);

            foreach (var claim in obj.Claims)
            {
                var claimEnt = await _repoClaim.Create(claim);
                claimEnt.UserId = obj.UserId;
                claimEnt.CreatedBy = obj.UserId;
                claimEnt.UpdatedBy = obj.UserId;
                // _db.UserClaims.Add(claimEnt);
            }
        }

        await _repo.SaveChanges();
        return _mapper.Map<UserInfo>(entity);
    }

    public async Task<IEnumerable<UserSummaryInfo>> GetSummary(QueryParamModel model)
    {
        if (model.Filter == null)
        {
            model.Filter = new List<FilterModel>();
        }
        var archiveFilter = model.Filter.Where(f => f.Property != "archived");
        var predicateArchive = GetFilter<User>(archiveFilter);

        var predicate = GetFilter<User>(model.Filter);

        var searchString = model.searchString;

        if (!string.IsNullOrEmpty(searchString))
        {
            predicate = GetSearchContain<User>(["userId", "name"], searchString);
            predicateArchive = GetSearchContain<User>(["userId", "name"], searchString);
        }

        var query = _db.Users.Where(predicate);


        var data = await query.GroupBy(x => x.Archived).Select(g => new UserSummaryInfo
        {
            Archived = g.Key,
            Count = g.Count()
        }).ToListAsync();

        return _mapper.Map<IEnumerable<UserSummaryInfo>>(data);
    }

    // public async Task<bool> CheckExistName(PayItemInfo obj)
    // {
    //     var sassdb = GetCurrentDb();
    //     var query = _db.PayItems
    //         .Where(f => f.Saasdb == sassdb);
    //     query = query.Where(f => f.Itemname.ToUpper() == obj.Itemname.ToUpper());
    //     var data = await query.ToListAsync();
    //     if (obj.NbpayitemId != null)
    //     {
    //         if (data.Count > 1) return true;
    //         else return false;
    //     }
    //     else
    //     {
    //         if (data.Count > 0) return true;
    //         else return false;
    //     }
    // }

    public async Task<UserResultInfo> GetMe(string refreshToken)
    {
        var userId = await _db.RefreshTokens.Where(f => f.Token == refreshToken && f.IsRevoked == false).Select(f => f.UserId).FirstOrDefaultAsync();
        if (userId == null) return null;

        var user = await _repo.Query(f => f.UserId == userId).Select(item => new User
        {
            UserId = item.UserId,
            Name = item.Name,
            Telephone = item.Telephone,
            Email = item.Email,
            Claims = item.Claims
        }).FirstOrDefaultAsync();

        // var existingClaims = _http.HttpContext.User.Claims.ToList();
        // existingClaims.Add(new Claim("sub", user.UserId.ToString()));
        // var updatedIdentity = new ClaimsIdentity(existingClaims);

        // _http.HttpContext.User = new ClaimsPrincipal(updatedIdentity);

        return new UserResultInfo
        {
            UserId = user.UserId,
            Name = user.Name,
            ClaimType = user.Claims.Select(c => c.ClaimId).ToArray(),
            ClaimValue = user.Claims.Select(c => c.Claimvalue).ToArray()
        };
    }

    public async Task<IEnumerable<UserSearchProfileInfo>> GetSearchProfile(string formid)
    {
        var userId = GetUserId();
        var data = await _repoSearch.Query(f => f.UserId == userId && f.FormId == formid).ToListAsync();

        return _mapper.Map<IEnumerable<UserSearchProfileInfo>>(data);
    }

    public async Task<UserSearchProfileInfo> AddSearchProfile(UserSearchProfileInfo obj)
    {
        obj.UserId = GetUserId();
        var entity = await _repoSearch.Create(obj);
        await _repoSearch.SaveChanges();
        return _mapper.Map<UserSearchProfileInfo>(entity);
    }

    public async Task<UserSearchProfileInfo> UpdateSearchProfile(UserSearchProfileInfo obj)
    {
        var entity = await _repoSearch.FindByIdAndUpdate(obj, obj.Id);

        await _repoSearch.SaveChanges();
        return _mapper.Map<UserSearchProfileInfo>(entity);
    }

    public async Task<bool> DeleteSearchProfile(Guid id)
    {
        await _repoSearch.FindByIdAndDelete(id);
        await _repoSearch.SaveChanges();
        return true;
    }

    public async Task<bool> CheckExistSearchProfile(UserSearchProfileInfo obj)
    {
        var query = _repoSearch.Query(f => f.ProfileName.ToUpper() == obj.ProfileName.ToUpper());
        var data = await query.ToListAsync();
        if (data.Count > 0) return true;
        else return false;
    }

    public async Task<IEnumerable<UserComboInfo>> QueryCombo(QueryParamModel model, int? page, int? pageSize)
    {
        var query = ApplyFilter(_repo, model, ["userId", "name"]);
        query = query.Where(f => f.Archived == 0).Select(item => new User
        {
            UserId = item.UserId,
            Name = item.Name
        });

        if (model.Sort != null)
            query = ApplySort(query, model.Sort);


        var data = await query.Skip((page.Value - 1) * pageSize.Value)
             .Take(pageSize.Value).ToListAsync();


        return _mapper.Map<IEnumerable<UserComboInfo>>(data);
    }

    public async Task<int> QueryComboTotal(QueryParamModel model)
    {
        var qry = ApplyFilter(_repo, model, ["userId", "name"]);
        var c = await qry.CountAsync();
        return c;
    }

}



