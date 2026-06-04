namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public interface IUserRepository
{
    Task<bool> IsEmailInUseAsync(string email, CancellationToken cancellationToken);
    Task<bool> IsPhoneInUseAsync(string phone, CancellationToken cancellationToken);

    Task<User?> GetUserByIdAsync(UserId id, CancellationToken cancellationToken);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetUserByPhoneAsync(string phone, CancellationToken cancellationToken);

    Task<User> CreateUserAsync(User user, CancellationToken cancellationToken);

    Task<bool> DeleteUserByIdAsync(UserId id, CancellationToken cancellationToken);
}
