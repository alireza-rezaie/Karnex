using Application.Features.Billing;
using Application.Features.Meetings;
using Application.Features.Projects;
using Application.Features.Reminders;
using Application.Features.Sprints;
using Application.Features.Users;
using Application.Features.WorkItems;
using FluentValidation;

namespace Application.Features.Validators;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(12).Matches("^[A-Za-z0-9_-]+$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}

public sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}

public sealed class CreateWorkItemCommandValidator : AbstractValidator<CreateWorkItemCommand>
{
    public CreateWorkItemCommandValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(10000);
        RuleFor(x => x.StoryPoints).InclusiveBetween(0, 100).When(x => x.StoryPoints.HasValue);
        RuleFor(x => x.DueDate).GreaterThan(DateTime.MinValue).When(x => x.DueDate.HasValue);
    }
}

public sealed class UpdateWorkItemCommandValidator : AbstractValidator<UpdateWorkItemCommand>
{
    public UpdateWorkItemCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(10000);
        RuleFor(x => x.StoryPoints).InclusiveBetween(0, 100).When(x => x.StoryPoints.HasValue);
    }
}

public sealed class AddWorkItemCommentCommandValidator : AbstractValidator<AddWorkItemCommentCommand>
{
    public AddWorkItemCommentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(5000);
    }
}

public sealed class AddWorkItemAttachmentCommandValidator : AbstractValidator<AddWorkItemAttachmentCommand>
{
    public AddWorkItemAttachmentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.FileUrl).NotEmpty().MaximumLength(2048);
        RuleFor(x => x.ContentType).MaximumLength(150);
        RuleFor(x => x.Size).GreaterThan(0).LessThanOrEqualTo(50 * 1024 * 1024);
    }
}

public sealed class CreateSprintCommandValidator : AbstractValidator<CreateSprintCommand>
{
    public CreateSprintCommandValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Goal).MaximumLength(1000);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}

public sealed class UpdateSprintCommandValidator : AbstractValidator<UpdateSprintCommand>
{
    public UpdateSprintCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Goal).MaximumLength(1000);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}

public sealed class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.PriceMonthly).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PriceYearly).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Users).GreaterThanOrEqualTo(-1);
        RuleFor(x => x.Storage).NotEmpty().MaximumLength(50);
        RuleForEach(x => x.Features!).NotEmpty().MaximumLength(120).When(x => x.Features is not null);
    }
}

public sealed class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.PriceMonthly).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PriceYearly).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Users).GreaterThanOrEqualTo(-1);
        RuleFor(x => x.Storage).NotEmpty().MaximumLength(50);
        RuleForEach(x => x.Features!).NotEmpty().MaximumLength(120).When(x => x.Features is not null);
    }
}

public sealed class ChangeSubscriptionCommandValidator : AbstractValidator<ChangeSubscriptionCommand>
{
    public ChangeSubscriptionCommandValidator() => RuleFor(x => x.PlanId).GreaterThan(0);
}

public sealed class CreateMeetingCommandValidator : AbstractValidator<CreateMeetingCommand>
{
    public CreateMeetingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Location).MaximumLength(300);
        RuleFor(x => x.MeetingUrl).MaximumLength(2048);
        RuleFor(x => x.EndsAt).GreaterThan(x => x.StartsAt);
    }
}

public sealed class UpdateMeetingCommandValidator : AbstractValidator<UpdateMeetingCommand>
{
    public UpdateMeetingCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Location).MaximumLength(300);
        RuleFor(x => x.MeetingUrl).MaximumLength(2048);
        RuleFor(x => x.EndsAt).GreaterThan(x => x.StartsAt);
    }
}

public sealed class CreateReminderCommandValidator : AbstractValidator<CreateReminderCommand>
{
    public CreateReminderCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ProjectId).GreaterThan(0).When(x => x.ProjectId.HasValue);
        RuleFor(x => x.WorkItemId).GreaterThan(0).When(x => x.WorkItemId.HasValue);
    }
}

public sealed class UpdateReminderCommandValidator : AbstractValidator<UpdateReminderCommand>
{
    public UpdateReminderCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ProjectId).GreaterThan(0).When(x => x.ProjectId.HasValue);
        RuleFor(x => x.WorkItemId).GreaterThan(0).When(x => x.WorkItemId.HasValue);
    }
}

public sealed class CreateManagedUserCommandValidator : AbstractValidator<CreateManagedUserCommand>
{
    public CreateManagedUserCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.Role).NotEmpty().MaximumLength(50);
    }
}

public sealed class UpdateManagedUserCommandValidator : AbstractValidator<UpdateManagedUserCommand>
{
    public UpdateManagedUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(256);
    }
}
