namespace Application.Dtos.Account;

public record ManagedUserDto(string Id, string DisplayName, string? Email, string? PhoneNumber, string? UserName, IReadOnlyList<string> Roles, int TaskCount, int ProjectCount, bool IsLocked);
