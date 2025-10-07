using Baluma.Emblue.ApiConsumer.Domain.Reports;
using Baluma.Emblue.ApiConsumer.Domain.TaskExecution;
using Microsoft.EntityFrameworkCore;

namespace Baluma.Emblue.ApiConsumer.Infrastructure.Persistence;

public sealed class ApiConsumerDbContext : DbContext
{
    public ApiConsumerDbContext(DbContextOptions<ApiConsumerDbContext> options) : base(options)
    {
    }

    public DbSet<DailyActivityDetail> DailyActivityDetails => Set<DailyActivityDetail>();
    public DbSet<DailyActionSummary> DailyActionSummaries => Set<DailyActionSummary>();
    public DbSet<TaskExecutionLog> TaskExecutionLogs => Set<TaskExecutionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DailyActivityDetail>(entity =>
        {
            entity.ToTable("EMB_PRE_DailyActivityDetail");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Campaign).HasMaxLength(256);
            entity.Property(e => e.Action).HasMaxLength(256);
            entity.Property(e => e.ActionType).HasMaxLength(128);
            entity.Property(e => e.Activity).HasMaxLength(128);
            entity.Property(e => e.Description).HasMaxLength(512);
            entity.Property(e => e.Tag).HasMaxLength(128);
            entity.Property(e => e.Account).HasMaxLength(128);
            entity.Property(e => e.Category).HasMaxLength(128);
            entity.Property(e => e.SegmentCategory).HasMaxLength(128);
        });

        modelBuilder.Entity<DailyActionSummary>(entity =>
        {
            entity.ToTable("EMB_PRE_DailyActionSummary");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Campaign).HasMaxLength(256);
            entity.Property(e => e.Action).HasMaxLength(256);
            entity.Property(e => e.Type).HasMaxLength(128);
            entity.Property(e => e.Subject).HasMaxLength(512);
            entity.Property(e => e.Sender).HasMaxLength(256);
            entity.Property(e => e.TrustedSender).HasMaxLength(64);
            entity.Property(e => e.TouchRules).HasMaxLength(128);
            entity.Property(e => e.Recipients).HasMaxLength(256);
            entity.Property(e => e.Sent).HasMaxLength(256);
            entity.Property(e => e.Bounces).HasMaxLength(256);
            entity.Property(e => e.Effective).HasMaxLength(256);
            entity.Property(e => e.Opens).HasMaxLength(256);
            entity.Property(e => e.UniqueOpens).HasMaxLength(256);
            entity.Property(e => e.Clicks).HasMaxLength(256);
            entity.Property(e => e.UniqueClicks).HasMaxLength(256);
            entity.Property(e => e.Virals).HasMaxLength(256);
            entity.Property(e => e.Subscribers).HasMaxLength(256);
            entity.Property(e => e.Unsubscribers).HasMaxLength(256);
            entity.Property(e => e.Dr).HasMaxLength(64);
            entity.Property(e => e.Br).HasMaxLength(64);
            entity.Property(e => e.Or).HasMaxLength(64);
            entity.Property(e => e.Uor).HasMaxLength(64);
            entity.Property(e => e.Ctr).HasMaxLength(64);
            entity.Property(e => e.Ctor).HasMaxLength(64);
            entity.Property(e => e.Ctuor).HasMaxLength(64);
            entity.Property(e => e.Vr).HasMaxLength(64);
        });

        modelBuilder.Entity<TaskExecutionLog>(entity =>
        {
            entity.ToTable("EMB_PRE_TaskExecutionLog");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaskName).HasMaxLength(128);
            entity.Property(e => e.Parameters).HasMaxLength(512);
            entity.Property(e => e.Status).HasMaxLength(32);
            entity.Property(e => e.Message).HasMaxLength(1024);
        });
    }
}
