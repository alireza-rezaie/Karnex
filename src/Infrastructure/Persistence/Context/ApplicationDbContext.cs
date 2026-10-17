using Domain.Entities.Identity;
using Domain.Entities.ProjectManagement;
using Domain.Entities.Billing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Persistence.Context;

public class ApplicationDbContext : IdentityDbContext<User, Role, string, IdentityUserClaim<string>,
    UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    //for identity
    public DbSet<User> User => Set<User>();
    public DbSet<Role> Role => Set<Role>();
    public DbSet<UserRole> UserRole => Set<UserRole>();
    public DbSet<Address> Address => Set<Address>();

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<WorkItemLabel> WorkItemLabels => Set<WorkItemLabel>();
    public DbSet<WorkItemComment> WorkItemComments => Set<WorkItemComment>();
    public DbSet<WorkItemAttachment> WorkItemAttachments => Set<WorkItemAttachment>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<MeetingParticipant> MeetingParticipants => Set<MeetingParticipant>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<WorkflowDefinitionRecord> WorkflowDefinitions => Set<WorkflowDefinitionRecord>();
    public DbSet<ProjectWorkflowBoardSetting> ProjectWorkflowBoardSettings => Set<ProjectWorkflowBoardSetting>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<ProjectRoleDefinition> ProjectRoleDefinitions => Set<ProjectRoleDefinition>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    //public DbSet<Sender> Sender => Set<Sender>();
    //public DbSet<Receiver> Receiver => Set<Receiver>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        //if is deleted equal true continue
        modelBuilder.Entity<Address>().HasQueryFilter(x => x.IsDelete == false);
    }
}
