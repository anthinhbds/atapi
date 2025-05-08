using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using atmnr_api.Data;
using atmnr_api.Entities;

namespace atmnr_api.SignalRHub
{
    public class AlertHub : Hub
    {
        protected readonly AtDbContext _db;
        public AlertHub(AtDbContext db)
        {
            _db = db;
        }
        public async Task GetBadgetCountApartment(string apartmentId)
        {
            try
            {
                var apartmentEntity = await _db.Apartments.Where(f => f.ApartmentId == apartmentId)
            .Join(_db.Projects, a => a.ProjectId, p => p.ProjectId, (a, p) => new { a, p })
            .Join(_db.Users, m => m.a.UserId, u => u.UserId, (m, u) => new Apartment
            {
                ApartmentId = m.a.ApartmentId,
                UserId = m.a.UserId,
                User = u,
                ProjectId = m.a.ProjectId,
                Project = m.p,
                Bedroom = m.a.Bedroom,
                Notes = m.a.Notes,
                LastUpdate = m.a.LastUpdate
            })
            .FirstOrDefaultAsync();

                var userEntities = await _db.Users.Where(f => f.UserId != "CTY" && f.UserId != apartmentEntity.UserId && f.Archived == 0).ToListAsync();

                if (userEntities.Count > 0)
                {
                    var today = DateTime.UtcNow;
                    today = today.AddDays(-7);
                    var expiryDate = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Utc);

                    foreach (var userEntity in userEntities)
                    {
                        List<Notification> notiList = new List<Notification>();
                        var customerQry = _db.Customers.Where(f => f.UserId == userEntity.UserId && f.LastUpdate >= expiryDate);
                        // customerQry.Where(f => f.Project.IndexOf(apartmentEntity.ProjectId) != -1 || f.Bedroom.IndexOf(apartmentEntity.Bedroom.ToString()) != -1);
                        customerQry = customerQry.Where(f => f.Project.IndexOf(apartmentEntity.ProjectId) != -1);

                        var customers = await customerQry.Take(5).ToListAsync();

                        if (customers.Count > 0)
                        {
                            var notiInf = new Notification
                            {
                                NotificationId = Guid.NewGuid(),
                                UserId = userEntity.UserId,
                                Type = "A",
                                Payload = "",
                                Isread = false,
                                Createddate = DateTime.UtcNow,
                                Lastupdate = DateTime.UtcNow
                            };
                            foreach (var customer in customers)
                            {
                                notiInf.Payload += customer.Customername + ", ";
                            }
                            notiInf.Payload = notiInf.Payload.Substring(0, notiInf.Payload.Length - 2);
                            notiList.Add(notiInf);
                        }

                        List<object> returnData = new List<object>();
                        for (var i = 0; i < notiList.Count; i++)
                        {
                            notiList[i].Payload = apartmentEntity.User.Lastname + " vừa có chính chủ " + apartmentEntity.Project.Projectname + " hợp với nhu cầu khách hàng " + notiList[i].Payload;

                            var count = await _db.Notifications.CountAsync(f => f.Isread == false && f.UserId == notiList[i].UserId && f.Type == "A");
                            count++;

                            returnData.Add(new { userId = notiList[i].UserId, count = count });

                            _db.Notifications.Add(notiList[i]);
                        }
                        await _db.SaveChangesAsync();
                        await Clients.All.SendAsync("ReceiveApartmentNotice", returnData.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                var logFilePath = "/var/log/anthinh_error.log"; // Đổi path nếu cần
                await File.AppendAllTextAsync(logFilePath, $"{DateTime.UtcNow}: {ex.Message} {ex.StackTrace}\n");
                await File.AppendAllTextAsync(logFilePath, $"{DateTime.UtcNow}: {ex.InnerException.Message} {ex.StackTrace}\n");
            }
        }

        public async Task SendApartmentExpire()
        {
            List<string> returnData = new List<string>();
            returnData.Add("test");
            await Clients.All.SendAsync("ReceiveApartmentExpire", returnData.ToArray());
        }
    }

}
