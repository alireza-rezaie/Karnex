using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Meetings;

public record CreateMeetingCommand(string Title, string? Description, string? Location, string? MeetingUrl, DateTime StartsAt, DateTime EndsAt, string? OrganizerId, int? ProjectId, IReadOnlyList<string>? ParticipantIds) : IRequest<MeetingDto>;
public record UpdateMeetingCommand(int Id, string Title, string? Description, string? Location, string? MeetingUrl, DateTime StartsAt, DateTime EndsAt, MeetingStatus Status, string? OrganizerId, int? ProjectId, IReadOnlyList<string>? ParticipantIds) : IRequest<MeetingDto>;
public record UpdateMeetingStatusCommand(int Id, MeetingStatus Status) : IRequest<MeetingDto>;
public record DeleteMeetingCommand(int Id) : IRequest<bool>;
