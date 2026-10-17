using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Persistence.Context;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Entities.Identity;
using Domain.Entities.Billing;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;


namespace Infrastructure.Persistence.SeedData;

public class GenerateFakeData
    {
        public static async Task SeedDataAsync(ApplicationDbContext context,ILoggerFactory loggerFactory) 
        {
			try
			{
            await SeedIdentityAsync(context);
            await SeedProjectManagementAsync(context);
            await SeedBillingAsync(context);
            await SeedSupplementalDataAsync(context);
                 

        }
			catch (Exception e)
			{
				var logger = loggerFactory.CreateLogger<GenerateFakeData>();
                logger.LogError(e,"Error In Seed Data");
            }
        }

    private static async Task SeedProjectManagementAsync(ApplicationDbContext context)
    {
        if (await context.Projects.AnyAsync())
        {
            await SeedRemindersAsync(context);
            await SeedMeetingsAsync(context);
            return;
        }

        var now = DateTime.UtcNow;
        var webProject = new Project
        {
            Key = "WEB",
            Name = "بازطراحی وب‌سایت",
            Description = "بازطراحی تجربه کاربری و اتصال صفحات اصلی وب‌سایت به API.",
            Type = ProjectType.Scrum,
            Status = ProjectStatus.Active,
            Privacy = ProjectPrivacy.Private,
            Icon = "globe",
            Color = "#3b82f6",
            StartDate = now.AddDays(-21),
            EndDate = now.AddDays(45),
            CreatedBy = "seed-admin"
        };
        webProject.Members.Add(new ProjectMember { UserId = "seed-admin", Role = "pm" });
        webProject.Members.Add(new ProjectMember { UserId = "seed-developer", Role = "Developer" });

        var webSprint = new Sprint
        {
            Name = "اسپرینت اول - احراز هویت",
            Goal = "تکمیل صفحات ورود و اتصال به API احراز هویت.",
            Status = SprintStatus.Active,
            StartDate = now.AddDays(-5),
            EndDate = now.AddDays(9),
            CreatedBy = "seed-admin"
        };
        webProject.Sprints.Add(webSprint);

        var authEpic = new WorkItem
        {
            Key = "WEB-1", Title = "احراز هویت کاربران", Description = "Epic مربوط به ورود، ثبت‌نام و بازیابی رمز عبور.",
            Type = WorkItemType.Epic, Status = WorkItemStatus.InProgress, Priority = WorkItemPriority.High,
            ReporterId = "seed-admin", AssigneeId = "seed-developer", Sprint = webSprint, StoryPoints = 13,
            DueDate = now.AddDays(7), CreatedBy = "seed-admin"
        };
        var loginTask = new WorkItem
        {
            Key = "WEB-2", Title = "اتصال فرم ورود به API", Description = "ارسال اطلاعات فرم ورود و مدیریت پاسخ سرویس.",
            Type = WorkItemType.Task, Status = WorkItemStatus.InProgress, Priority = WorkItemPriority.Highest,
            ReporterId = "seed-admin", AssigneeId = "seed-developer", Sprint = webSprint, Parent = authEpic,
            StoryPoints = 5, DueDate = now.AddDays(2), CreatedBy = "seed-admin"
        };
        loginTask.Labels.Add(new WorkItemLabel { Name = "API", Color = "#3b82f6" });
        loginTask.Labels.Add(new WorkItemLabel { Name = "Auth", Color = "#8b5cf6" });
        loginTask.Comments.Add(new WorkItemComment { AuthorId = "seed-admin", Content = "لطفاً مدیریت خطاهای اعتبارسنجی نیز انجام شود.", CreatedBy = "seed-admin" });
        loginTask.Attachments.Add(new WorkItemAttachment { FileName = "login-flow.pdf", FileUrl = "/seed-files/login-flow.pdf", ContentType = "application/pdf", Size = 245760, CreatedBy = "seed-admin" });
        var registerTask = new WorkItem
        {
            Key = "WEB-3", Title = "طراحی صفحه ثبت‌نام", Description = "طراحی واکنش‌گرا برای صفحه ثبت‌نام.",
            Type = WorkItemType.Task, Status = WorkItemStatus.Done, Priority = WorkItemPriority.Medium,
            ReporterId = "seed-admin", AssigneeId = "seed-developer", Sprint = webSprint, Parent = authEpic,
            StoryPoints = 3, DueDate = now.AddDays(-2), CompletedAt = now.AddDays(-1), CreatedBy = "seed-admin"
        };
        registerTask.Labels.Add(new WorkItemLabel { Name = "UI", Color = "#22c55e" });
        var searchBug = new WorkItem
        {
            Key = "WEB-4", Title = "رفع باگ جستجو", Description = "نتایج جستجو در صفحه موبایل نمایش داده نمی‌شوند.",
            Type = WorkItemType.Bug, Status = WorkItemStatus.Todo, Priority = WorkItemPriority.High,
            ReporterId = "seed-admin", AssigneeId = "seed-developer", StoryPoints = 2,
            DueDate = now.AddDays(-1), CreatedBy = "seed-admin"
        };
        searchBug.Labels.Add(new WorkItemLabel { Name = "Bug", Color = "#ef4444" });
        webProject.WorkItems.Add(authEpic);
        webProject.WorkItems.Add(loginTask);
        webProject.WorkItems.Add(registerTask);
        webProject.WorkItems.Add(searchBug);

        var mobileProject = new Project
        {
            Key = "APP", Name = "اپلیکیشن موبایل", Description = "توسعه اپلیکیشن Android و iOS برای مشتریان.",
            Type = ProjectType.Scrum, Status = ProjectStatus.Active, Privacy = ProjectPrivacy.Private,
            Icon = "smartphone", Color = "#f97316", StartDate = now.AddDays(-15), EndDate = now.AddDays(60), CreatedBy = "seed-admin"
        };
        mobileProject.Members.Add(new ProjectMember { UserId = "seed-mobile", Role = "Developer" });
        var mobileSprint = new Sprint { Name = "اسپرینت API", Goal = "اتصال قابلیت‌های اصلی به API.", Status = SprintStatus.Active, StartDate = now.AddDays(-2), EndDate = now.AddDays(12), CreatedBy = "seed-admin" };
        mobileProject.Sprints.Add(mobileSprint);
        mobileProject.WorkItems.Add(new WorkItem { Key = "APP-1", Title = "پیاده‌سازی API محصولات", Description = "اتصال فهرست محصولات اپلیکیشن به API.", Type = WorkItemType.Story, Status = WorkItemStatus.InProgress, Priority = WorkItemPriority.High, AssigneeId = "seed-mobile", ReporterId = "seed-admin", Sprint = mobileSprint, StoryPoints = 8, DueDate = now.AddDays(5), CreatedBy = "seed-admin" });
        mobileProject.WorkItems.Add(new WorkItem { Key = "APP-2", Title = "تست واحد سفارش‌ها", Description = "افزودن تست برای جریان ثبت سفارش.", Type = WorkItemType.Task, Status = WorkItemStatus.Todo, Priority = WorkItemPriority.Medium, AssigneeId = "seed-mobile", ReporterId = "seed-admin", Sprint = mobileSprint, StoryPoints = 5, DueDate = now.AddDays(8), CreatedBy = "seed-admin" });

        var crmProject = new Project
        {
            Key = "CRM", Name = "سیستم مدیریت مشتریان", Description = "مدیریت مشتریان و مخاطبان تیم فروش.",
            Type = ProjectType.Kanban, Status = ProjectStatus.Active, Privacy = ProjectPrivacy.Public,
            Icon = "users", Color = "#22c55e", StartDate = now.AddDays(-10), EndDate = now.AddDays(90), CreatedBy = "seed-admin"
        };
        crmProject.WorkItems.Add(new WorkItem { Key = "CRM-1", Title = "طراحی فرم ثبت مشتری", Description = "فرم جامع ثبت اطلاعات مشتری.", Type = WorkItemType.Task, Status = WorkItemStatus.Done, Priority = WorkItemPriority.High, ReporterId = "seed-admin", StoryPoints = 5, CompletedAt = now.AddDays(-2), CreatedBy = "seed-admin" });
        crmProject.WorkItems.Add(new WorkItem { Key = "CRM-2", Title = "ساخت API مخاطبین", Description = "CRUD مخاطبین همراه با اعتبارسنجی.", Type = WorkItemType.Task, Status = WorkItemStatus.Review, Priority = WorkItemPriority.High, ReporterId = "seed-admin", StoryPoints = 8, DueDate = now.AddDays(4), CreatedBy = "seed-admin" });

        await context.Projects.AddRangeAsync(webProject, mobileProject, crmProject);
        await context.SaveChangesAsync();

        await SeedRemindersAsync(context);
        await SeedMeetingsAsync(context);
    }

    private static async Task SeedIdentityAsync(ApplicationDbContext context)
    {
        var roles = new[] { (Id: "seed-role-admin", Name: "Admin"), (Id: "seed-role-user", Name: "User") };
        foreach (var role in roles)
            if (!await context.Role.AnyAsync(x => x.Name == role.Name))
                await context.Role.AddAsync(new Role { Id = role.Id, Name = role.Name, NormalizedName = role.Name.ToUpperInvariant(), ConcurrencyStamp = Guid.NewGuid().ToString() });

        var users = new[] {
            (Id: "seed-admin", Name: "مدیر کارنکس", Phone: "09120000001", Email: "admin@karnex.local", RoleId: "seed-role-admin"),
            (Id: "seed-developer", Name: "علی توسعه‌دهنده", Phone: "09120000002", Email: "developer@karnex.local", RoleId: "seed-role-user"),
            (Id: "seed-mobile", Name: "سارا موبایل", Phone: "09120000003", Email: "mobile@karnex.local", RoleId: "seed-role-user")
        };
        var hasher = new PasswordHasher<User>();
        foreach (var item in users)
        {
            if (await context.User.AnyAsync(x => x.Id == item.Id || x.PhoneNumber == item.Phone)) continue;
            var user = new User { Id = item.Id, DisplayName = item.Name, UserName = item.Phone, NormalizedUserName = item.Phone, PhoneNumber = item.Phone, PhoneNumberConfirmed = true, Email = item.Email, NormalizedEmail = item.Email.ToUpperInvariant(), EmailConfirmed = true, SecurityStamp = Guid.NewGuid().ToString(), ConcurrencyStamp = Guid.NewGuid().ToString() };
            user.PasswordHash = hasher.HashPassword(user, "Karnex!123");
            await context.User.AddAsync(user);
            await context.UserRole.AddAsync(new UserRole(item.RoleId, item.Id));
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedBillingAsync(ApplicationDbContext context)
    {
        if (!await context.Plans.AnyAsync())
        {
            await context.Plans.AddRangeAsync(
                new Plan { Name = "رایگان", Description = "برای شروع تیم‌های کوچک", PriceMonthly = 0, PriceYearly = 0, Users = 3, Storage = "1 GB", FeaturesJson = JsonSerializer.Serialize(new[] { "پروژه‌های پایه", "برد کانبان" }), DisplayOrder = 1, Active = true },
                new Plan { Name = "حرفه‌ای", Description = "برای تیم‌های در حال رشد", PriceMonthly = 590000, PriceYearly = 5900000, Users = 20, Storage = "25 GB", FeaturesJson = JsonSerializer.Serialize(new[] { "اسپرینت", "گزارش‌ها", "دسترسی‌های پیشرفته" }), DisplayOrder = 2, Highlighted = true, Active = true },
                new Plan { Name = "سازمانی", Description = "برای سازمان‌های بزرگ", PriceMonthly = 1990000, PriceYearly = 19900000, Users = -1, Storage = "250 GB", FeaturesJson = JsonSerializer.Serialize(new[] { "کاربر نامحدود", "گزارش سازمانی", "پشتیبانی ویژه" }), DisplayOrder = 3, Active = true });
            await context.SaveChangesAsync();
        }
        if (!await context.UserSubscriptions.AnyAsync(x => x.UserId == "seed-admin"))
        {
            var plan = await context.Plans.OrderByDescending(x => x.DisplayOrder).FirstAsync();
            await context.UserSubscriptions.AddAsync(new UserSubscription { UserId = "seed-admin", PlanId = plan.Id, Status = "active", PeriodStart = DateTime.UtcNow.AddDays(-10), PeriodEnd = DateTime.UtcNow.AddDays(20), CreatedBy = "seed-admin" });
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedRemindersAsync(ApplicationDbContext context)
    {
        if (await context.Reminders.AnyAsync()) return;

        var project = await context.Projects.OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (project is null) return;

        var workItem = await context.WorkItems
            .Where(x => x.ProjectId == project.Id)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();
        var now = DateTime.UtcNow;

        await context.Reminders.AddRangeAsync(
            new Reminder { Title = "تکمیل اتصال فرم ورود", Description = "بررسی پاسخ‌های خطای API احراز هویت.", ReminderAt = now.AddHours(2), Priority = WorkItemPriority.High, UserId = "seed-admin", ProjectId = project.Id, WorkItemId = workItem?.Id, CreatedBy = "seed-admin" },
            new Reminder { Title = "جلسه بررسی اسپرینت", Description = "مرور وضعیت کارهای اسپرینت فعال.", ReminderAt = now.AddHours(5), Priority = WorkItemPriority.Medium, UserId = "seed-admin", ProjectId = project.Id, CreatedBy = "seed-admin" },
            new Reminder { Title = "بازبینی وظایف پروژه", Description = "وضعیت وظایف نزدیک به موعد را بررسی کنید.", ReminderAt = now.AddDays(1).AddHours(9), Priority = WorkItemPriority.Low, UserId = "seed-admin", ProjectId = project.Id, CreatedBy = "seed-admin" });
        await context.SaveChangesAsync();
    }

    private static async Task SeedMeetingsAsync(ApplicationDbContext context)
    {
        if (await context.Meetings.AnyAsync()) return;

        var project = await context.Projects.OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (project is null) return;

        var today = DateTime.UtcNow.Date;
        await context.Meetings.AddRangeAsync(
            new Meeting
            {
                Title = "جلسه روزانه اسکرام", Description = "مرور کارهای انجام‌شده، برنامه امروز و موانع تیم.", Location = "اتاق جلسه آنلاین", MeetingUrl = "https://meet.google.com/", StartsAt = today.AddHours(9), EndsAt = today.AddHours(9.5), OrganizerId = "seed-admin", ProjectId = project.Id, CreatedBy = "seed-admin",
                Participants = { new MeetingParticipant { UserId = "seed-admin" }, new MeetingParticipant { UserId = "seed-developer" } }
            },
            new Meeting
            {
                Title = "بازبینی کد", Description = "بررسی Pull Requestهای آماده برای انتشار.", Location = "Google Meet", MeetingUrl = "https://meet.google.com/", StartsAt = today.AddHours(14), EndsAt = today.AddHours(15), OrganizerId = "seed-admin", ProjectId = project.Id, CreatedBy = "seed-admin",
                Participants = { new MeetingParticipant { UserId = "seed-admin" }, new MeetingParticipant { UserId = "seed-developer" } }
            },
            new Meeting
            {
                Title = "ارائه به مشتری", Description = "نمایش نسخه جدید و دریافت بازخورد مشتری.", Location = "اتاق کنفرانس", StartsAt = today.AddHours(16.5), EndsAt = today.AddHours(17.5), OrganizerId = "seed-admin", ProjectId = project.Id, CreatedBy = "seed-admin",
                Participants = { new MeetingParticipant { UserId = "seed-admin" } }
            });
        await context.SaveChangesAsync();
    }

    // Keeps development databases useful even after the initial seed has run.
    // Every generated key/name is deterministic, so restarting the API never duplicates data.
    private static async Task SeedSupplementalDataAsync(ApplicationDbContext context)
    {
        var project = await context.Projects.OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (project is null) return;
        var now = DateTime.UtcNow;
        var users = (await context.User.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync()).DefaultIfEmpty("seed-admin").ToList();
        var reminderTitles = new[] { "تأیید طراحی صفحه ورود", "ارسال نسخه آزمایشی به QA", "پیگیری بازخورد مشتری", "آماده‌سازی جلسه برنامه‌ریزی", "بررسی خطاهای Sentry", "مرور Pull Requestهای باز", "به‌روزرسانی مستندات API", "تحویل گزارش اسپرینت", "بررسی ظرفیت تیم", "برنامه‌ریزی انتشار نسخه ۱.۲" };
        var meetingTitles = new[] { "دیلی اسکرام تیم وب", "جلسه برنامه‌ریزی اسپرینت", "بازبینی طراحی رابط کاربری", "جلسه رفع موانع تیم", "مرور نتایج تست QA", "بازبینی کد سرویس احراز هویت", "هماهنگی انتشار نسخه", "جلسه دمو با مشتری", "رتروسپکتیو اسپرینت", "تحلیل شاخص‌های محصول" };
        var notificationTitles = new[] { "وظیفه جدید به شما واگذار شد", "یک نظر جدید ثبت شد", "اسپرینت فردا به پایان می‌رسد", "درخواست تأیید شما ثبت شد", "فایل جدید در پروژه بارگذاری شد", "مهلت وظیفه نزدیک است", "عضویت شما در پروژه تغییر کرد", "جلسه امروز یادآوری شد", "گزارش هفتگی آماده است", "نسخه جدید Workflow منتشر شد" };
        var teamNames = new[] { "تیم توسعه وب", "تیم موبایل", "تیم محصول", "تیم طراحی تجربه کاربری", "تیم تضمین کیفیت", "تیم زیرساخت و DevOps", "تیم پشتیبانی مشتریان", "تیم تحلیل داده", "تیم فروش سازمانی", "تیم امنیت" };
        var roleNames = new[] { "تحلیل‌گر کسب‌وکار", "مالک محصول", "توسعه‌دهنده فرانت‌اند", "توسعه‌دهنده بک‌اند", "مهندس QA", "طراح UI/UX", "مهندس DevOps", "ناظر پروژه", "کارشناس پشتیبانی", "بازبین کد" };
        var projectNames = new[] { "بهبود تجربه ثبت‌نام", "داشبورد مدیریتی", "اتوماسیون پشتیبانی", "پرتال مشتریان", "یکپارچه‌سازی پیامک", "بهینه‌سازی جستجو", "مهاجرت زیرساخت", "تحلیل رفتار کاربران", "بازطراحی مرکز اعلان", "سامانه گزارش‌گیری" };
        var projectDescriptions = new[] { "کاهش مراحل ثبت‌نام و افزایش نرخ تکمیل ایجاد حساب کاربری.", "نمایش شاخص‌های کلیدی پروژه و وضعیت تیم برای مدیران سازمان.", "کاهش زمان پاسخ‌گویی با مسیردهی و اولویت‌بندی خودکار درخواست‌ها.", "فراهم‌کردن دسترسی مشتریان به وضعیت سفارش‌ها، فاکتورها و درخواست‌ها.", "ارسال پیامک‌های تراکنشی برای ورود، یادآوری و اطلاع‌رسانی وضعیت.", "بهبود دقت و سرعت جست‌وجو در پروژه‌ها، وظایف و کاربران.", "انتقال سرویس‌ها به زیرساخت پایدار با مانیتورینگ و نسخه‌برداری منظم.", "اندازه‌گیری مسیرهای کاربر برای بهبود قابلیت‌های پرکاربرد محصول.", "یکپارچه‌سازی اعلان‌های درون‌برنامه‌ای، ایمیل و یادآورها.", "ارائه گزارش‌های تصمیم‌ساز برای عملکرد پروژه و ظرفیت تیم." };
        var meetingDescriptions = new[] { "مرور کارهای دیروز، برنامهٔ امروز و موانع اعضای تیم.", "تخمین کارها، تعیین هدف اسپرینت و نهایی‌کردن ظرفیت تیم.", "بررسی جریان‌های اصلی محصول و تصمیم‌گیری دربارهٔ جزئیات طراحی.", "شناسایی موانع فنی و تعیین مسئول و زمان رفع آن‌ها.", "مرور سناریوهای ناموفق تست و اولویت‌بندی رفع باگ‌ها.", "بازبینی کیفیت پیاده‌سازی، امنیت و پوشش تست تغییرات اخیر.", "بررسی چک‌لیست انتشار و مسئولیت‌های قبل و بعد از Deploy.", "نمایش قابلیت‌های تکمیل‌شده و ثبت بازخورد ذی‌نفعان.", "بررسی موارد قابل بهبود و تعریف اقدام‌های عملی برای اسپرینت بعد.", "تحلیل روند استفاده و شاخص‌های کلیدی عملکرد محصول." };

        var projectCount = await context.Projects.CountAsync();
        for (var i = projectCount; i < 10; i++)
        {
            var item = new Project { Key = $"DEM{i + 1:00}", Name = projectNames[i], Description = projectDescriptions[i], Type = (ProjectType)(i % 3), Privacy = i % 2 == 0 ? ProjectPrivacy.Private : ProjectPrivacy.Public, Icon = "folder", Color = new[] { "#3b82f6", "#22c55e", "#f97316", "#8b5cf6" }[i % 4], StartDate = now.AddDays(-20 + i), EndDate = now.AddDays(35 + i), CreatedBy = "seed-admin" };
            item.Members.Add(new ProjectMember { UserId = users[i % users.Count], Role = i % 3 == 0 ? "pm" : "member" }); context.Projects.Add(item);
        }
        await context.SaveChangesAsync();
        var projects = await context.Projects.OrderBy(x => x.Id).Take(10).ToListAsync();
        var sprintCount = await context.Sprints.CountAsync();
        for (var i = sprintCount; i < 10; i++) context.Sprints.Add(new Sprint { ProjectId = projects[i % projects.Count].Id, Name = $"اسپرینت انتشار {i + 1}", Goal = "تکمیل و انتشار قابلیت‌های اولویت‌دار محصول.", Status = (SprintStatus)(i % 3), StartDate = now.AddDays(i * 14), EndDate = now.AddDays(i * 14 + 13), CreatedBy = "seed-admin" });
        await context.SaveChangesAsync();
        var workCount = await context.WorkItems.CountAsync();
        for (var i = workCount; i < 10; i++) context.WorkItems.Add(new WorkItem { ProjectId = projects[i % projects.Count].Id, Key = $"{projects[i % projects.Count].Key}-{i + 1}", Title = $"تکمیل قابلیت محصول {i + 1}", Description = "وظیفهٔ نمونه با معیار پذیرش، مسئول و زمان‌بندی مشخص.", Type = (WorkItemType)(i % 4), Status = (WorkItemStatus)(i % 4), Priority = (WorkItemPriority)(i % 4), AssigneeId = users[i % users.Count], ReporterId = "seed-admin", StoryPoints = (i % 8) + 1, DueDate = now.AddDays(i + 3), CreatedBy = "seed-admin", CompletedAt = i % 4 == 3 ? now.AddDays(-i) : null });
        await context.SaveChangesAsync();
        var workItems = await context.WorkItems.OrderBy(x => x.Id).Take(10).ToListAsync();
        for (var i = await context.WorkItemLabels.CountAsync(); i < 10; i++) context.WorkItemLabels.Add(new WorkItemLabel { WorkItemId = workItems[i % workItems.Count].Id, Name = new[] { "backend", "frontend", "qa", "urgent", "design" }[i % 5], Color = "#3b82f6" });
        for (var i = await context.Set<WorkItemComment>().CountAsync(); i < 10; i++) context.Set<WorkItemComment>().Add(new WorkItemComment { WorkItemId = workItems[i % workItems.Count].Id, AuthorId = users[i % users.Count], Content = "وضعیت بررسی شد؛ اقدام بعدی در برنامهٔ روزانه پیگیری می‌شود.", CreatedBy = users[i % users.Count] });
        for (var i = await context.WorkItemAttachments.CountAsync(); i < 10; i++) context.WorkItemAttachments.Add(new WorkItemAttachment { WorkItemId = workItems[i % workItems.Count].Id, FileName = $"specification-{i + 1}.pdf", FileUrl = $"/seed-files/specification-{i + 1}.pdf", ContentType = "application/pdf", Size = 125000 + i * 1000, CreatedBy = "seed-admin" });

        var reminders = await context.Reminders.CountAsync();
        for (var i = reminders; i < 10; i++) context.Reminders.Add(new Reminder { Title = reminderTitles[i], Description = "یادآور عملیاتی برای پیگیری فعالیت‌های پروژه.", ReminderAt = now.AddDays(i + 1).AddHours(9), Priority = (WorkItemPriority)(i % 4), UserId = users[i % users.Count], ProjectId = project.Id, CreatedBy = "seed-admin" });
        var meetings = await context.Meetings.CountAsync();
        for (var i = meetings; i < 10; i++) context.Meetings.Add(new Meeting { Title = meetingTitles[i], Description = meetingDescriptions[i], Location = i % 2 == 0 ? "Google Meet" : "اتاق جلسه طبقه دوم", StartsAt = now.Date.AddDays(i).AddHours(9), EndsAt = now.Date.AddDays(i).AddHours(10), OrganizerId = "seed-admin", ProjectId = project.Id, CreatedBy = "seed-admin", Participants = { new MeetingParticipant { UserId = users[i % users.Count] } } });
        var notifications = await context.UserNotifications.CountAsync();
        for (var i = notifications; i < 10; i++) context.UserNotifications.Add(new UserNotification { UserId = users[i % users.Count], Title = notificationTitles[i], Body = "برای مشاهدهٔ جزئیات و اقدام لازم، وارد پروژه شوید.", Channel = i % 3 == 0 ? "email" : "in_app", IsRead = i % 3 == 0, CreatedBy = "seed-admin" });
        foreach (var userId in users) if (!await context.NotificationPreferences.AnyAsync(x => x.UserId == userId)) context.NotificationPreferences.Add(new NotificationPreference { UserId = userId, PreferencesJson = "{\"in_app\":true,\"email\":true,\"push\":false}", CreatedBy = "seed-admin" });
        var teams = await context.Teams.CountAsync();
        for (var i = teams; i < 10; i++) { var team = new Team { Name = teamNames[i], Description = "تیم مسئول تحویل بخش‌های کلیدی محصول کارنکس.", LeadId = users[i % users.Count], CreatedBy = "seed-admin" }; foreach (var userId in users.Take(2)) team.Members.Add(new TeamMember { UserId = userId }); context.Teams.Add(team); }
        var roles = await context.ProjectRoleDefinitions.CountAsync();
        for (var i = roles; i < 10; i++) context.ProjectRoleDefinitions.Add(new ProjectRoleDefinition { ProjectId = project.Id, Name = roleNames[i], Description = "نقش سفارشی پروژه با سطح دسترسی تعریف‌شده.", PermissionsJson = "[[true,true,false,false,false],[true,true,true,false,false]]", CreatedBy = "seed-admin" });
        var oldReminders = await context.Reminders.Where(x => x.Title.StartsWith("یادآور نمونه")).OrderBy(x => x.Id).Take(10).ToListAsync(); for (var i = 0; i < oldReminders.Count; i++) oldReminders[i].Title = reminderTitles[i];
        var oldMeetings = await context.Meetings.Where(x => x.Title.StartsWith("جلسه نمونه") || x.Description == "جلسهٔ هماهنگی و تصمیم‌گیری برای پیشبرد فعالیت‌های پروژه.").OrderBy(x => x.Id).Take(10).ToListAsync(); for (var i = 0; i < oldMeetings.Count; i++) { oldMeetings[i].Title = meetingTitles[i]; oldMeetings[i].Description = meetingDescriptions[i]; }
        var oldNotifications = await context.UserNotifications.Where(x => x.Title.StartsWith("اعلان نمونه")).OrderBy(x => x.Id).Take(10).ToListAsync(); for (var i = 0; i < oldNotifications.Count; i++) oldNotifications[i].Title = notificationTitles[i];
        var oldTeams = await context.Teams.Where(x => x.Name.StartsWith("تیم نمونه")).OrderBy(x => x.Id).Take(10).ToListAsync(); for (var i = 0; i < oldTeams.Count; i++) oldTeams[i].Name = teamNames[i];
        var oldRoles = await context.ProjectRoleDefinitions.Where(x => x.Name.StartsWith("نقش نمونه")).OrderBy(x => x.Id).Take(10).ToListAsync(); for (var i = 0; i < oldRoles.Count; i++) oldRoles[i].Name = roleNames[i];
        var oldProjects = await context.Projects.Where(x => x.Description == "پروژهٔ نمونه با داده‌های واقع‌گرایانه برای محیط توسعه.").OrderBy(x => x.Id).Take(10).ToListAsync(); for (var i = 0; i < oldProjects.Count; i++) oldProjects[i].Description = projectDescriptions[i];
        var genericTasks = await context.WorkItems.Where(x => x.Description == "وظیفهٔ نمونه با معیار پذیرش، مسئول و زمان‌بندی مشخص.").ToListAsync(); foreach (var item in genericTasks) item.Description = "پیاده‌سازی قابلیت موردنیاز، ثبت تست‌های لازم و آماده‌سازی برای بازبینی کد.";
        var genericReminders = await context.Reminders.Where(x => x.Description == "یادآور عملیاتی برای پیگیری فعالیت‌های پروژه.").ToListAsync(); foreach (var item in genericReminders) item.Description = "برای جلوگیری از تأخیر در تحویل، این مورد در جلسهٔ روزانه پیگیری شود.";
        var genericNotifications = await context.UserNotifications.Where(x => x.Body == "برای مشاهدهٔ جزئیات و اقدام لازم، وارد پروژه شوید.").ToListAsync(); foreach (var item in genericNotifications) item.Body = "این رویداد نیازمند بررسی شماست؛ برای مشاهدهٔ جزئیات به بخش مربوطه مراجعه کنید.";
        var genericTeams = await context.Teams.Where(x => x.Description == "تیم مسئول تحویل بخش‌های کلیدی محصول کارنکس.").ToListAsync(); foreach (var item in genericTeams) item.Description = "مسئول برنامه‌ریزی، اجرای کارهای اولویت‌دار و تحویل خروجی‌های این حوزه.";
        var genericRoles = await context.ProjectRoleDefinitions.Where(x => x.Description == "نقش سفارشی پروژه با سطح دسترسی تعریف‌شده.").ToListAsync(); foreach (var item in genericRoles) item.Description = "دسترسی‌های این نقش مطابق مسئولیت فرد در چرخهٔ اجرای پروژه تنظیم شده است.";
        await context.SaveChangesAsync();
    }
    }

