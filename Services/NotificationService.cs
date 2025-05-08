

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

public interface INotificationService
{
    Task<IEnumerable<NotificationInfo>> Get();
    Task<int> GetTotal();
    Task<bool> MarkRead(Guid id);
}
public class NotificationService : GenericService, INotificationService
{
    protected readonly IMapper _mapper;
    public NotificationService(IHttpContextAccessor http, AtDbContext db, IMapper mapper)
    : base(http, db)
    {
        _mapper = mapper;
    }
    public async Task<IEnumerable<NotificationInfo>> Get()
    {
        var userId = GetUserId();
        var query = _db.Notifications.Where(f => f.UserId == userId);
        var data = await query.Take(15).ToListAsync();
        return _mapper.Map<IEnumerable<NotificationInfo>>(data);
    }
    public async Task<int> GetTotal()
    {
        var userId = GetUserId();
        var query = _db.Notifications.Where(f => f.UserId == userId && f.Isread == false);
        var c = await query.CountAsync();
        return c;
    }


    public async Task<bool> MarkRead(Guid id)
    {
        var entity = _db.Notifications.Where(f => f.NotificationId == id).FirstOrDefault();
        if (entity != null)
        {
            entity.Isread = true;
            entity.Lastupdate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }
        return false;

    }

}



