
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;
using atmnr_api.Entities;

namespace atmnr_api.Data;

public class AtDbContext : DbContext
{
    public AtDbContext(DbContextOptions<AtDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>();
        builder.Entity<UserClaim>();
        builder.Entity<UserSearchProfile>();
        builder.Entity<RefreshToken>();
        builder.Entity<Project>();
        builder.Entity<District>();
        builder.Entity<Ward>();
        builder.Entity<Apartment>();
        builder.Entity<ApartmentNote>().HasKey(x => new { x.ApartmentId, x.Linenum });
        builder.Entity<Customer>();
        builder.Entity<CustomerNote>().HasKey(x => new { x.CustomerId, x.Linenum });
        builder.Entity<AssignmentLog>();
        builder.Entity<Transaction>();
        builder.Entity<TransactionDetail>().HasKey(x => new { x.TransId, x.Linenum });
        builder.Entity<TransactionMember>().HasKey(x => new { x.TransId, x.UserId });
        builder.Entity<Notification>();
        builder.Entity<History>();
        builder.Entity<CustomerJourney>();
        builder.Entity<CustomerJourneyDet>().HasKey(x => new { x.CustomerId, x.Journeydate });
        builder.Entity<ViewStatistic>();
        builder.Entity<Scheduler>();

        base.OnModelCreating(builder);
    }
    public DbSet<User> Users { get; set; }
    public DbSet<UserClaim> UserClaims { get; set; }
    public DbSet<UserSearchProfile> UserSearchProfiles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<Ward> Wards { get; set; }
    public DbSet<Apartment> Apartments { get; set; }
    public DbSet<ApartmentNote> ApartmentNotes { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerNote> CustomerNotes { get; set; }
    public DbSet<AssignmentLog> AssignmentLogs { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionDetail> TransactionDetails { get; set; }
    public DbSet<TransactionMember> TransactionMembers { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<History> Histories { get; set; }
    public DbSet<CustomerJourney> CustomerJourneys { get; set; }
    public DbSet<CustomerJourneyDet> CustomerJourneyDets { get; set; }
    public DbSet<ViewStatistic> ViewStatistics { get; set; }
    public DbSet<Scheduler> Schedulers { get; set; }
}