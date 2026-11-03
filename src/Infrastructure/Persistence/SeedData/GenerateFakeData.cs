using System.Text.Json;
using Domain.Entities.Billing;
using Domain.Entities.Identity;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.SeedData;

/// <summary>
/// Deterministic, idempotent development data for every application-owned table.
/// This seeder never deletes or overwrites user-created records.
/// </summary>
public static class GenerateFakeData
{
    private const string SeedTag = "karnex-dev-seed";
    private const string AdminId = "seed-admin";
    private const string DeveloperId = "seed-developer";
    private const string DesignerId = "seed-designer";
    private const string QaId = "seed-qa";

    public static async Task SeedDataAsync(ApplicationDbContext context, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(context);
        var logger = loggerFactory.CreateLogger("Karnex.SeedData");

        try
        {
            // Each phase saves its own parents before creating dependent rows.
            // Re-running this method is safe: natural keys and unique indexes are checked.
            var users = await SeedIdentityAsync(context);
            var plans = await SeedBillingAsync(context, users);
            var projects = await SeedProjectsAsync(context, users);
            var sprints = await SeedSprintsAsync(context, projects);
            var workItems = await SeedWorkItemsAsync(context, projects, sprints, users);
            await SeedWorkItemChildrenAsync(context, workItems, users);
            await SeedTeamsAsync(context, users);
            await SeedProjectRolesAsync(context, projects);
            var workflowId = await SeedWorkflowAsync(context);
            await SeedBoardSettingsAsync(context, projects, workflowId);
            await SeedMeetingsAsync(context, projects, users);
            await SeedRemindersAsync(context, projects, workItems, users);
            await SeedNotificationsAsync(context, users);
            await SeedSubscriptionsAsync(context, plans, users);

            logger.LogInformation(
                "Karnex development seed completed. Users={Users}, Plans={Plans}, Projects={Projects}, Sprints={Sprints}, WorkItems={WorkItems}, Teams={Teams}, Meetings={Meetings}, Reminders={Reminders}, Notifications={Notifications}",
                await context.User.CountAsync(), await context.Plans.CountAsync(),
                await context.Projects.CountAsync(), await context.Sprints.CountAsync(),
                await context.WorkItems.CountAsync(), await context.Teams.CountAsync(),
                await context.Meetings.CountAsync(), await context.Reminders.CountAsync(),
                await context.UserNotifications.CountAsync());
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Karnex development seed failed.");
            throw;
        }
    }

    private static async Task<Dictionary<string, User>> SeedIdentityAsync(ApplicationDbContext db)
    {
        var roleDefinitions = new[]
        {
            (Name: "Admin", Id: "seed-role-admin"),
            (Name: "User", Id: "seed-role-user")
        };

        var roles = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in roleDefinitions)
        {
            var role = await db.Role.SingleOrDefaultAsync(x => x.Name == definition.Name);
            if (role is null)
            {
                role = new Role
                {
                    Id = definition.Id,
                    Name = definition.Name,
                    NormalizedName = definition.Name.ToUpperInvariant(),
                    ConcurrencyStamp = Guid.NewGuid().ToString("N")
                };
                db.Role.Add(role);
                await db.SaveChangesAsync();
            }
            roles[definition.Name] = role;
        }

        var password = Environment.GetEnvironmentVariable("KARNEX_SEED_PASSWORD");
        if (string.IsNullOrWhiteSpace(password))
            password = "Karnex.Dev!2026";

        var definitions = new[]
        {
            new SeedUser(AdminId, "مدیر سیستم کارنکس", "09120000001", "admin@karnex.local", "Admin"),
            new SeedUser(DeveloperId, "علی توسعه‌دهنده", "09120000002", "developer@karnex.local", "User"),
            new SeedUser(DesignerId, "سارا طراح محصول", "09120000003", "designer@karnex.local", "User"),
            new SeedUser(QaId, "محمد کارشناس کیفیت", "09120000004", "qa@karnex.local", "User")
        };

        var hasher = new PasswordHasher<User>();
        var result = new Dictionary<string, User>();
        foreach (var definition in definitions)
        {
            var user = await db.User.SingleOrDefaultAsync(x => x.Id == definition.Id)
                ?? await db.User.SingleOrDefaultAsync(x => x.PhoneNumber == definition.Phone)
                ?? await db.User.SingleOrDefaultAsync(x => x.Email == definition.Email);

            if (user is null)
            {
                user = new User
                {
                    Id = definition.Id,
                    UserName = definition.Phone,
                    NormalizedUserName = definition.Phone,
                    PhoneNumber = definition.Phone,
                    PhoneNumberConfirmed = true,
                    Email = definition.Email,
                    NormalizedEmail = definition.Email.ToUpperInvariant(),
                    EmailConfirmed = true,
                    DisplayName = definition.DisplayName,
                    SecurityStamp = Guid.NewGuid().ToString("N"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("N"),
                    LockoutEnabled = true
                };
                user.PasswordHash = hasher.HashPassword(user, password);
                db.User.Add(user);
                await db.SaveChangesAsync();
            }
            else if (string.IsNullOrWhiteSpace(user.DisplayName))
            {
                user.DisplayName = definition.DisplayName;
                await db.SaveChangesAsync();
            }

            var role = roles[definition.Role];
            var hasRole = await db.UserRole.AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id);
            if (!hasRole)
            {
                db.UserRole.Add(new UserRole(role.Id, user.Id));
                await db.SaveChangesAsync();
            }
            result[definition.Key] = user;

            // Seed one address per account without touching existing addresses.
            if (!await db.Address.AnyAsync(x => x.UserId == user.Id))
            {
                db.Address.Add(new Address
                {
                    UserId = user.Id,
                    IsMain = true,
                    State = "تهران",
                    City = "تهران",
                    FirstName = definition.DisplayName.Split(' ')[0],
                    LastName = definition.DisplayName.Split(' ').Last(),
                    FullAddress = "خیابان نمونه، پلاک ۱۲، واحد ۳",
                    Number = "02100000000",
                    PostalCode = "1234567890"
                });
                await db.SaveChangesAsync();
            }
        }

        return result;
    }

    private static async Task<Dictionary<string, Plan>> SeedBillingAsync(ApplicationDbContext db, IReadOnlyDictionary<string, User> users)
    {
        var definitions = new[]
        {
            new PlanSeed("free", "رایگان", "برای آشنایی و تیم‌های کوچک", 0, 0, 3, "2 GB", new[] { "پروژه‌های پایه", "برد کانبان", "گزارش‌های پایه" }, 1, false),
            new PlanSeed("pro", "حرفه‌ای", "برای تیم‌های در حال رشد", 590000, 5900000, 20, "25 GB", new[] { "پروژه نامحدود", "اسپرینت", "گزارش پیشرفته", "گردش‌کار سفارشی" }, 2, true),
            new PlanSeed("business", "سازمانی", "برای سازمان‌های چندتیمی", 1990000, 19900000, -1, "250 GB", new[] { "کاربر نامحدود", "مدیریت تیم", "گزارش سازمانی", "پشتیبانی اولویت‌دار" }, 3, false)
        };

        var result = new Dictionary<string, Plan>();
        foreach (var definition in definitions)
        {
            var plan = await db.Plans.SingleOrDefaultAsync(x => x.Name == definition.Name);
            if (plan is null)
            {
                plan = new Plan
                {
                    Name = definition.Name,
                    Description = definition.Description,
                    PriceMonthly = definition.Monthly,
                    PriceYearly = definition.Yearly,
                    Users = definition.Users,
                    Storage = definition.Storage,
                    FeaturesJson = JsonSerializer.Serialize(definition.Features),
                    DisplayOrder = definition.Order,
                    Highlighted = definition.Highlighted,
                    Active = true,
                    CreatedBy = SeedTag
                };
                db.Plans.Add(plan);
                await db.SaveChangesAsync();
            }
            result[definition.Key] = plan;
        }
        return result;
    }

    private static async Task<Dictionary<string, Project>> SeedProjectsAsync(ApplicationDbContext db, IReadOnlyDictionary<string, User> users)
    {
        var definitions = new[]
        {
            new ProjectSeed("WEB", "پلتفرم وب کارنکس", "توسعه و بهبود پنل مدیریت پروژه و تجربه کاربری.", ProjectType.Scrum, "#3b82f6"),
            new ProjectSeed("APP", "اپلیکیشن موبایل", "توسعه نسخه‌های موبایل و اتصال سرویس‌های اصلی.", ProjectType.Kanban, "#f97316"),
            new ProjectSeed("CRM", "سامانه مدیریت مشتریان", "مدیریت مشتریان، ارتباطات و گزارش‌های فروش.", ProjectType.Basic, "#22c55e")
        };

        var result = new Dictionary<string, Project>();
        foreach (var definition in definitions)
        {
            var project = await db.Projects.SingleOrDefaultAsync(x => x.Key == definition.Key);
            if (project is null)
            {
                project = new Project
                {
                    Key = definition.Key,
                    Name = definition.Name,
                    Description = definition.Description,
                    Type = definition.Type,
                    Status = ProjectStatus.Active,
                    Privacy = ProjectPrivacy.Private,
                    Icon = definition.Key == "APP" ? "smartphone" : definition.Key == "CRM" ? "users" : "globe",
                    Color = definition.Color,
                    StartDate = DateTime.UtcNow.Date.AddDays(-14),
                    EndDate = DateTime.UtcNow.Date.AddDays(90),
                    CreatedBy = users[AdminId].Id
                };
                db.Projects.Add(project);
                await db.SaveChangesAsync();
            }

            var memberships = definition.Key == "WEB"
                ? new[] { (users[AdminId], "pm"), (users[DeveloperId], "member"), (users[DesignerId], "member"), (users[QaId], "member") }
                : definition.Key == "APP"
                    ? new[] { (users[AdminId], "pm"), (users[DeveloperId], "member"), (users[QaId], "member") }
                    : new[] { (users[AdminId], "pm"), (users[DesignerId], "member") };

            foreach (var (user, role) in memberships)
            {
                if (!await db.ProjectMembers.AnyAsync(x => x.ProjectId == project.Id && x.UserId == user.Id))
                    db.ProjectMembers.Add(new ProjectMember { ProjectId = project.Id, UserId = user.Id, Role = role });
            }
            await db.SaveChangesAsync();
            result[definition.Key] = project;
        }
        return result;
    }

    private static async Task<Dictionary<string, Sprint>> SeedSprintsAsync(ApplicationDbContext db, IReadOnlyDictionary<string, Project> projects)
    {
        var result = new Dictionary<string, Sprint>();
        foreach (var (key, project) in projects)
        {
            var sprintName = $"{key} - اسپرینت اول";
            var sprint = await db.Sprints.SingleOrDefaultAsync(x => x.ProjectId == project.Id && x.Name == sprintName);
            if (sprint is null)
            {
                var now = DateTime.UtcNow.Date;
                sprint = new Sprint
                {
                    ProjectId = project.Id,
                    Name = sprintName,
                    Goal = "تحویل قابلیت‌های اولویت‌دار با کیفیت قابل انتشار.",
                    Status = key == "WEB" ? SprintStatus.Active : SprintStatus.Planned,
                    StartDate = now.AddDays(-3),
                    EndDate = now.AddDays(11),
                    CreatedBy = AdminId
                };
                db.Sprints.Add(sprint);
                await db.SaveChangesAsync();
            }
            result[key] = sprint;
        }
        return result;
    }

    private static async Task<Dictionary<string, WorkItem>> SeedWorkItemsAsync(
        ApplicationDbContext db,
        IReadOnlyDictionary<string, Project> projects,
        IReadOnlyDictionary<string, Sprint> sprints,
        IReadOnlyDictionary<string, User> users)
    {
        var definitions = new[]
        {
            new WorkSeed("WEB", "WEB-1", "تکمیل احراز هویت", WorkItemType.Epic, WorkItemStatus.InProgress, WorkItemPriority.High, DeveloperId, 13),
            new WorkSeed("WEB", "WEB-2", "اتصال فرم ورود به API", WorkItemType.Task, WorkItemStatus.InProgress, WorkItemPriority.Highest, DeveloperId, 5),
            new WorkSeed("WEB", "WEB-3", "طراحی صفحه ثبت‌نام", WorkItemType.Story, WorkItemStatus.Done, WorkItemPriority.Medium, DesignerId, 3),
            new WorkSeed("WEB", "WEB-4", "تست سناریوهای احراز هویت", WorkItemType.Bug, WorkItemStatus.Todo, WorkItemPriority.High, QaId, 3),
            new WorkSeed("APP", "APP-1", "اتصال اپلیکیشن به API", WorkItemType.Story, WorkItemStatus.InProgress, WorkItemPriority.High, DeveloperId, 8),
            new WorkSeed("APP", "APP-2", "تست واحد سفارش‌ها", WorkItemType.Task, WorkItemStatus.Todo, WorkItemPriority.Medium, QaId, 5),
            new WorkSeed("CRM", "CRM-1", "طراحی فرم ثبت مشتری", WorkItemType.Task, WorkItemStatus.Done, WorkItemPriority.Medium, DesignerId, 5),
            new WorkSeed("CRM", "CRM-2", "ساخت API مخاطبین", WorkItemType.Task, WorkItemStatus.Review, WorkItemPriority.High, DeveloperId, 8)
        };

        var result = new Dictionary<string, WorkItem>();
        foreach (var definition in definitions)
        {
            var project = projects[definition.ProjectKey];
            var item = await db.WorkItems.SingleOrDefaultAsync(x => x.ProjectId == project.Id && x.Key == definition.Key);
            if (item is null)
            {
                item = new WorkItem
                {
                    ProjectId = project.Id,
                    SprintId = definition.ProjectKey == "CRM" ? null : sprints[definition.ProjectKey].Id,
                    Key = definition.Key,
                    Title = definition.Title,
                    Description = "شرح وظیفه، معیار پذیرش و مراحل انجام در این بخش ثبت می‌شود.",
                    Type = definition.Type,
                    Status = definition.Status,
                    Priority = definition.Priority,
                    AssigneeId = users[definition.AssigneeKey].Id,
                    ReporterId = users[AdminId].Id,
                    StoryPoints = definition.Points,
                    DueDate = DateTime.UtcNow.Date.AddDays(7),
                    CompletedAt = definition.Status == WorkItemStatus.Done ? DateTime.UtcNow : null,
                    CreatedBy = users[AdminId].Id
                };
                db.WorkItems.Add(item);
                await db.SaveChangesAsync();
            }
            result[definition.Key] = item;
        }
        return result;
    }

    private static async Task SeedWorkItemChildrenAsync(ApplicationDbContext db, IReadOnlyDictionary<string, WorkItem> items, IReadOnlyDictionary<string, User> users)
    {
        foreach (var (key, item) in items)
        {
            if (!await db.WorkItemLabels.AnyAsync(x => x.WorkItemId == item.Id))
            {
                db.WorkItemLabels.AddRange(
                    new WorkItemLabel { WorkItemId = item.Id, Name = "backend", Color = "#3b82f6" },
                    new WorkItemLabel { WorkItemId = item.Id, Name = "development", Color = "#8b5cf6" });
            }
            if (!await db.WorkItemComments.AnyAsync(x => x.WorkItemId == item.Id))
                db.WorkItemComments.Add(new WorkItemComment
                {
                    WorkItemId = item.Id,
                    AuthorId = users[AdminId].Id,
                    Content = $"وضعیت {key} بررسی شد؛ نتیجه در بازبینی بعدی پیگیری می‌شود.",
                    CreatedBy = users[AdminId].Id
                });
            if (!await db.WorkItemAttachments.AnyAsync(x => x.WorkItemId == item.Id))
                db.WorkItemAttachments.Add(new WorkItemAttachment
                {
                    WorkItemId = item.Id,
                    FileName = $"{key.ToLowerInvariant()}-specification.pdf",
                    FileUrl = $"/seed-files/{key.ToLowerInvariant()}-specification.pdf",
                    ContentType = "application/pdf",
                    Size = 128000,
                    CreatedBy = users[AdminId].Id
                });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedTeamsAsync(ApplicationDbContext db, IReadOnlyDictionary<string, User> users)
    {
        var definitions = new[]
        {
            ("تیم توسعه وب", "توسعه و نگهداری سرویس‌های وب.", DeveloperId, new[] { AdminId, DeveloperId, QaId }),
            ("تیم محصول و طراحی", "تحلیل نیازمندی‌ها و طراحی تجربه کاربری.", DesignerId, new[] { AdminId, DesignerId }),
            ("تیم تضمین کیفیت", "تست، کنترل کیفیت و بررسی انتشار.", QaId, new[] { AdminId, QaId, DeveloperId })
        };
        foreach (var definition in definitions)
        {
            var team = await db.Teams.SingleOrDefaultAsync(x => x.Name == definition.Item1);
            if (team is null)
            {
                team = new Team { Name = definition.Item1, Description = definition.Item2, LeadId = users[definition.Item3].Id, CreatedBy = users[AdminId].Id };
                db.Teams.Add(team);
                await db.SaveChangesAsync();
            }
            foreach (var memberKey in definition.Item4)
                if (!await db.TeamMembers.AnyAsync(x => x.TeamId == team.Id && x.UserId == users[memberKey].Id))
                    db.TeamMembers.Add(new TeamMember { TeamId = team.Id, UserId = users[memberKey].Id });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedProjectRolesAsync(ApplicationDbContext db, IReadOnlyDictionary<string, Project> projects)
    {
        var definitions = new[]
        {
            ("مالک محصول", "مدیریت اولویت‌ها و معیار پذیرش.", "[[true,true,true,false,true]]"),
            ("توسعه‌دهنده", "اجرای وظایف فنی و ثبت تغییرات.", "[[true,true,false,false,false]]"),
            ("ناظر", "مشاهده وضعیت و گزارش‌های پروژه.", "[[true,false,false,false,false]]")
        };
        foreach (var project in projects.Values)
        foreach (var definition in definitions)
        {
            if (!await db.ProjectRoleDefinitions.AnyAsync(x => x.ProjectId == project.Id && x.Name == definition.Item1))
                db.ProjectRoleDefinitions.Add(new ProjectRoleDefinition
                {
                    ProjectId = project.Id,
                    Name = definition.Item1,
                    Description = definition.Item2,
                    PermissionsJson = definition.Item3,
                    CreatedBy = AdminId
                });
        }
        await db.SaveChangesAsync();
    }

    private static async Task<int> SeedWorkflowAsync(ApplicationDbContext db)
    {
        const string workflowName = "Karnex Default Workflow";
        var workflow = await db.WorkflowDefinitions.SingleOrDefaultAsync(x => x.Name == workflowName && x.IsSystem);
        if (workflow is null)
        {
            workflow = new WorkflowDefinitionRecord
            {
                Name = workflowName,
                Description = "گردش‌کار پیش‌فرض کارنکس برای محیط توسعه.",
                IsSystem = true,
                IsDraft = false,
                Version = 1,
                PayloadJson = JsonSerializer.Serialize(new
                {
                    states = new[]
                    {
                        new { id = "todo", name = "To do", category = "todo" },
                        new { id = "in-progress", name = "In progress", category = "inprogress" },
                        new { id = "review", name = "Review", category = "review" },
                        new { id = "done", name = "Done", category = "done" }
                    },
                    transitions = new[]
                    {
                        new { from = "todo", to = "in-progress" },
                        new { from = "in-progress", to = "review" },
                        new { from = "review", to = "done" },
                        new { from = "review", to = "in-progress" }
                    }
                }),
                CreatedBy = AdminId
            };
            db.WorkflowDefinitions.Add(workflow);
            await db.SaveChangesAsync();
        }
        return workflow.Id;
    }

    private static async Task SeedBoardSettingsAsync(ApplicationDbContext db, IReadOnlyDictionary<string, Project> projects, int workflowId)
    {
        var columns = JsonSerializer.Serialize(new[]
        {
            new { id = "todo", title = "To do", order = 0 },
            new { id = "in-progress", title = "In progress", order = 1 },
            new { id = "review", title = "Review", order = 2 },
            new { id = "done", title = "Done", order = 3 }
        });
        foreach (var project in projects.Values)
        {
            var setting = await db.ProjectWorkflowBoardSettings.SingleOrDefaultAsync(x => x.ProjectId == project.Id);
            if (setting is null)
                db.ProjectWorkflowBoardSettings.Add(new ProjectWorkflowBoardSetting
                {
                    ProjectId = project.Id,
                    WorkflowDefinitionRecordId = workflowId,
                    KanbanBacklogEnabled = true,
                    BoardColumnsJson = columns,
                    CreatedBy = AdminId
                });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedMeetingsAsync(ApplicationDbContext db, IReadOnlyDictionary<string, Project> projects, IReadOnlyDictionary<string, User> users)
    {
        foreach (var (key, project) in projects)
        {
            var title = $"{key} - جلسه برنامه‌ریزی";
            var meeting = await db.Meetings.SingleOrDefaultAsync(x => x.ProjectId == project.Id && x.Title == title);
            if (meeting is null)
            {
                var start = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
                meeting = new Meeting
                {
                    ProjectId = project.Id,
                    OrganizerId = users[AdminId].Id,
                    Title = title,
                    Description = "مرور پیشرفت، موانع و برنامه اقدام اعضای تیم.",
                    Location = "اتاق جلسه آنلاین",
                    MeetingUrl = "https://meet.google.com/",
                    StartsAt = start,
                    EndsAt = start.AddHours(1),
                    Status = MeetingStatus.Scheduled,
                    CreatedBy = users[AdminId].Id
                };
                db.Meetings.Add(meeting);
                await db.SaveChangesAsync();
            }
            foreach (var userKey in new[] { AdminId, DeveloperId })
                if (!await db.MeetingParticipants.AnyAsync(x => x.MeetingId == meeting.Id && x.UserId == users[userKey].Id))
                    db.MeetingParticipants.Add(new MeetingParticipant { MeetingId = meeting.Id, UserId = users[userKey].Id, HasAccepted = userKey == AdminId });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedRemindersAsync(ApplicationDbContext db, IReadOnlyDictionary<string, Project> projects, IReadOnlyDictionary<string, WorkItem> items, IReadOnlyDictionary<string, User> users)
    {
        var definitions = new[]
        {
            ("یادآوری بررسی وظایف", "WEB", "WEB-2", AdminId, WorkItemPriority.High),
            ("یادآوری تست نسخه", "APP", "APP-2", QaId, WorkItemPriority.Medium),
            ("یادآوری بازبینی مشتری", "CRM", "CRM-2", DesignerId, WorkItemPriority.Low)
        };
        foreach (var definition in definitions)
        {
            var title = $"{definition.Item2} - {definition.Item1}";
            if (!await db.Reminders.AnyAsync(x => x.Title == title && x.UserId == users[definition.Item4].Id))
                db.Reminders.Add(new Reminder
                {
                    Title = title,
                    Description = "این مورد را در جلسه روزانه بررسی و نتیجه را ثبت کنید.",
                    ReminderAt = DateTime.UtcNow.AddDays(1),
                    Priority = definition.Item5,
                    UserId = users[definition.Item4].Id,
                    ProjectId = projects[definition.Item2].Id,
                    WorkItemId = items[definition.Item3].Id,
                    CreatedBy = users[AdminId].Id
                });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedNotificationsAsync(ApplicationDbContext db, IReadOnlyDictionary<string, User> users)
    {
        var notifications = new[]
        {
            (AdminId, "به‌روزرسانی پروژه", "پروژه وب به‌روزرسانی شد.", "in_app"),
            (DeveloperId, "وظیفه جدید", "وظیفه اتصال فرم ورود به شما واگذار شد.", "in_app"),
            (DesignerId, "بازبینی طراحی", "طرح صفحه ثبت‌نام آماده بازبینی است.", "email"),
            (QaId, "درخواست تست", "نسخه جدید برای تست آماده است.", "in_app")
        };
        foreach (var definition in notifications)
        {
            var userId = users[definition.Item1].Id;
            if (!await db.UserNotifications.AnyAsync(x => x.UserId == userId && x.Title == definition.Item2))
                db.UserNotifications.Add(new UserNotification
                {
                    UserId = userId,
                    Title = definition.Item2,
                    Body = definition.Item3,
                    Channel = definition.Item4,
                    IsRead = false,
                    CreatedBy = users[AdminId].Id
                });
            if (!await db.NotificationPreferences.AnyAsync(x => x.UserId == userId))
                db.NotificationPreferences.Add(new NotificationPreference
                {
                    UserId = userId,
                    PreferencesJson = JsonSerializer.Serialize(new { in_app = true, email = true, push = false }),
                    CreatedBy = users[AdminId].Id
                });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedSubscriptionsAsync(ApplicationDbContext db, IReadOnlyDictionary<string, Plan> plans, IReadOnlyDictionary<string, User> users)
    {
        var assignments = new[]
        {
            (AdminId, "business"),
            (DeveloperId, "pro"),
            (DesignerId, "free"),
            (QaId, "free")
        };
        foreach (var (userKey, planKey) in assignments)
        {
            var userId = users[userKey].Id;
            if (!await db.UserSubscriptions.AnyAsync(x => x.UserId == userId))
                db.UserSubscriptions.Add(new UserSubscription
                {
                    UserId = userId,
                    PlanId = plans[planKey].Id,
                    Status = "active",
                    PeriodStart = DateTime.UtcNow.Date,
                    PeriodEnd = DateTime.UtcNow.Date.AddMonths(1),
                    CancelAtPeriodEnd = false,
                    CreatedBy = users[AdminId].Id
                });
        }
        await db.SaveChangesAsync();
    }

    private sealed record SeedUser(string Key, string DisplayName, string Phone, string Email, string Role);
    private sealed record PlanSeed(string Key, string Name, string Description, decimal Monthly, decimal Yearly, int Users, string Storage, string[] Features, int Order, bool Highlighted);
    private sealed record ProjectSeed(string Key, string Name, string Description, ProjectType Type, string Color);
    private sealed record WorkSeed(string ProjectKey, string Key, string Title, WorkItemType Type, WorkItemStatus Status, WorkItemPriority Priority, string AssigneeKey, int Points);
}
