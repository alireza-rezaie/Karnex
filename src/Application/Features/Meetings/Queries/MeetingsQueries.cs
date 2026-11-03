using Application.Contracts;
using Application.Dtos.ProjectManagement;
using Domain.Entities.ProjectManagement;
using Domain.Enums;
using Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Meetings;

public record GetMeetingsQuery(DateTime? Date, int? ProjectId) : IRequest<IReadOnlyList<MeetingDto>>;
public record GetMeetingByIdQuery(int Id) : IRequest<MeetingDto>;
