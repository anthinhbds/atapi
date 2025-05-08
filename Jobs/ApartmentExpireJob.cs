using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Cronos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using atmnr_api.Data;
using atmnr_api.Entities;
using atmnr_api.Models;
using atmnr_api.SignalRHub;

public class ApartmentExpireJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApartmentExpireJob> _logger;
    private readonly CronExpression _cron;
    private DateTimeOffset? _nextRun;
    private readonly IHubContext<AlertHub> _hubContext;


    public ApartmentExpireJob(IServiceProvider serviceProvider, ILogger<ApartmentExpireJob> logger, IHubContext<AlertHub> hubContext)
    {
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
        // CRON: every 5 minutes
        _cron = CronExpression.Parse("*/15 * * * *", CronFormat.Standard);
    }

    private DateTime CalculateNextRun(Scheduler scheduler)
    {
        if (scheduler.Isrepeat == "Y")
        {
            return DateTime.UtcNow;
        }
        else
        {
            if (scheduler.Periodtype == "D")
            {
                DateTime nextRun = new DateTime(
                scheduler.Nextrunning.Year,
                scheduler.Nextrunning.Month,
                scheduler.Nextrunning.Day,
                scheduler.Hour,
                scheduler.Minute,
                0).AddDays(1);
                return nextRun;
            }
            else if (scheduler.Periodtype == "W")
            {
                DateTime nextRun = new DateTime(
                scheduler.Nextrunning.Year,
                scheduler.Nextrunning.Month,
                scheduler.Day,
                scheduler.Hour,
                scheduler.Minute,
                0).AddDays(7);
                return nextRun;
            }
            else if (scheduler.Periodtype == "M")
            {
                DateTime nextRun = new DateTime(
                scheduler.Nextrunning.Year,
                scheduler.Nextrunning.Month,
                scheduler.Day,
                scheduler.Hour,
                scheduler.Minute,
                0).AddMonths(1);
                return nextRun;
            }
            return DateTime.UtcNow;
        }
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _nextRun ??= _cron.GetNextOccurrence(DateTime.UtcNow);

            if (_nextRun.HasValue && _nextRun.Value <= DateTimeOffset.Now)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AtDbContext>();

                try
                {
                    //Automatic process apartment expired
                    var apartmentExpEnt = await dbContext.Schedulers.Where(f => f.SchedulerId == "APARTMENT_EXPIRE").FirstOrDefaultAsync();
                    if (apartmentExpEnt != null)
                    {

                        DateTime today = DateTime.Now;
                        DateTime expiredDate = DateTime.SpecifyKind(apartmentExpEnt.Nextrunning.AddMonths(-3), DateTimeKind.Utc);
                        if (today >= apartmentExpEnt.Nextrunning)
                        {
                            var apartments = await dbContext.Apartments.Where(f => f.LastUpdate <= expiredDate && f.UserId != "CTY").ToListAsync();
                            var nextRunning = CalculateNextRun(apartmentExpEnt);
                            apartmentExpEnt.Lastrun = DateTime.SpecifyKind(apartmentExpEnt.Nextrunning.AddHours(-7), DateTimeKind.Utc);
                            apartmentExpEnt.Nextrunning = DateTime.SpecifyKind(nextRunning.AddHours(-7), DateTimeKind.Utc);

                            History log = new History
                            {
                                LogId = Guid.NewGuid(),
                                Actiondate = DateTime.UtcNow,
                                FormId = "APT",
                                Contentlog = $"Chuyển tự động {apartments.Count} chính chủ quá hạn",
                                UserId = "CTY",
                                Actiontype = "U",
                                ReferenceId = "",
                            };
                            dbContext.Histories.Add(log);

                            foreach (var apartment in apartments)
                            {
                                // var apartment = apartments[0];
                                apartment.Expireddate = expiredDate;
                                apartment.PrevioususerId = apartment.UserId;
                                apartment.UserId = "CTY";
                                apartment.UpdatedBy = "CTY";
                            }
                            dbContext.SaveChanges();
                        }
                    }

                    //Call signalR to notify the client apartment will expire
                    var notiEnt = await dbContext.Schedulers.Where(f => f.SchedulerId == "NOTI_APARTMENT_EXPIRE").FirstOrDefaultAsync();
                    if (notiEnt != null)
                    {
                        DateTime today = DateTime.Now;
                        DateTime markDate = new DateTime(today.Year, today.Month, 25, 0, 0, 0);
                        if (today >= notiEnt.Nextrunning)
                        {
                            var nextRunning = CalculateNextRun(notiEnt);
                            notiEnt.Lastrun = DateTime.SpecifyKind(notiEnt.Nextrunning.AddHours(-7), DateTimeKind.Utc);
                            notiEnt.Nextrunning = DateTime.SpecifyKind(nextRunning.AddHours(-7), DateTimeKind.Utc);
                            dbContext.SaveChanges();

                            TimeSpan s = markDate - today;
                            if (s.Days <= 7)
                            {
                                string[] states = ["H"];
                                DateTime expiredDate = DateTime.SpecifyKind(markDate.AddDays(-3), DateTimeKind.Utc);
                                var groupedApartments = await dbContext.Apartments.Where(f => f.LastUpdate <= expiredDate && f.UserId != "CTY" && !states.Contains(f.Status))
                                .GroupBy(f => f.UserId)
                                .Select(s => new
                                {
                                    s.Key,
                                    Count = s.Count()
                                }).ToListAsync();
                                await _hubContext.Clients.All.SendAsync("SendApartmentExpire", groupedApartments);

                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running scheduled task.");
                }

                _nextRun = _cron.GetNextOccurrence(DateTime.UtcNow);
            }

            await Task.Delay(1000, stoppingToken); // Check every second
        }
    }
}
