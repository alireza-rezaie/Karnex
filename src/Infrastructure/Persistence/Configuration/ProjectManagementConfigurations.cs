using Domain.Entities.ProjectManagement;
using Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.Property(x => x.Key).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.Key).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.Property(x => x.Color).HasMaxLength(20);
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}

public class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.Property(x => x.Key).HasMaxLength(40).IsRequired();
        builder.HasIndex(x => new { x.ProjectId, x.Key }).IsUnique();
        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(10000);
        builder.HasOne(x => x.Project).WithMany(x => x.WorkItems).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Sprint).WithMany(x => x.WorkItems).HasForeignKey(x => x.SprintId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Parent).WithMany(x => x.Subtasks).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}

public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
{
    public void Configure(EntityTypeBuilder<Sprint> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Goal).HasMaxLength(4000);
        builder.HasOne(x => x.Project).WithMany(x => x.Sprints).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Role).HasMaxLength(100);
        builder.HasOne(x => x.Project).WithMany(x => x.Members).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.UserId).HasMaxLength(450);
        builder.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.WorkItem).WithMany().HasForeignKey(x => x.WorkItemId).OnDelete(DeleteBehavior.SetNull);
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.Location).HasMaxLength(500);
        builder.Property(x => x.MeetingUrl).HasMaxLength(2000);
        builder.Property(x => x.OrganizerId).HasMaxLength(450);
        builder.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}

public class MeetingParticipantConfiguration : IEntityTypeConfiguration<MeetingParticipant>
{
    public void Configure(EntityTypeBuilder<MeetingParticipant> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.HasIndex(x => new { x.MeetingId, x.UserId }).IsUnique();
        builder.HasOne(x => x.Meeting).WithMany(x => x.Participants).HasForeignKey(x => x.MeetingId).OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}

public class WorkflowDefinitionRecordConfiguration : IEntityTypeConfiguration<WorkflowDefinitionRecord>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinitionRecord> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PayloadJson).IsRequired();
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}

public class WorkflowPublishedVersionRecordConfiguration : IEntityTypeConfiguration<WorkflowPublishedVersionRecord>
{
    public void Configure(EntityTypeBuilder<WorkflowPublishedVersionRecord> builder)
    {
        builder.Property(x => x.PayloadJson).IsRequired();
        builder.HasIndex(x => new { x.WorkflowDefinitionRecordId, x.Version }).IsUnique();
        builder.HasOne(x => x.Workflow).WithMany(x => x.PublishedVersions)
            .HasForeignKey(x => x.WorkflowDefinitionRecordId).OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}

public class ProjectWorkflowBoardSettingConfiguration : IEntityTypeConfiguration<ProjectWorkflowBoardSetting>
{
    public void Configure(EntityTypeBuilder<ProjectWorkflowBoardSetting> builder)
    {
        builder.HasIndex(x => x.ProjectId).IsUnique();
        builder.Property(x => x.BoardColumnsJson).IsRequired();
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.LeadId).HasMaxLength(450);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}
public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.HasIndex(x => new { x.TeamId, x.UserId }).IsUnique();
        builder.HasOne(x => x.Team).WithMany(x => x.Members).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Cascade);
    }
}
public class ProjectRoleDefinitionConfiguration : IEntityTypeConfiguration<ProjectRoleDefinition>
{
    public void Configure(EntityTypeBuilder<ProjectRoleDefinition> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.PermissionsJson).IsRequired();
        builder.HasIndex(x => new { x.ProjectId, x.Name }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}
public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification> { public void Configure(EntityTypeBuilder<UserNotification> b) { b.Property(x => x.UserId).HasMaxLength(450).IsRequired(); b.Property(x => x.Title).HasMaxLength(500).IsRequired(); b.Property(x => x.Channel).HasMaxLength(50).IsRequired(); b.HasIndex(x => new { x.UserId, x.IsRead }); b.HasQueryFilter(x => !x.IsDelete); } }
public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference> { public void Configure(EntityTypeBuilder<NotificationPreference> b) { b.Property(x => x.UserId).HasMaxLength(450).IsRequired(); b.HasIndex(x => x.UserId).IsUnique(); b.HasQueryFilter(x => !x.IsDelete); } }

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.Storage).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FeaturesJson).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.PriceMonthly).HasPrecision(18, 2);
        builder.Property(x => x.PriceYearly).HasPrecision(18, 2);
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDelete);
    }
}
