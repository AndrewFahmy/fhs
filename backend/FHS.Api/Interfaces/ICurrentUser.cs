namespace FHS.Api.Interfaces;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? SubjectId { get; }
}
