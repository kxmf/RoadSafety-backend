using System.Net.Mail;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public interface IUserRepository
{
    Task<bool> IsEmailInUseAsync(MailAddress email, CancellationToken cancellationToken);
    Task<bool> IsPhoneInUseAsync(PhoneNumber phone, CancellationToken cancellationToken);

    Task<User?> GetUserByIdAsync(UserId id, CancellationToken cancellationToken);
    Task<User?> GetUserByEmailAsync(MailAddress email, CancellationToken cancellationToken);
    Task<User?> GetUserByPhoneAsync(PhoneNumber phone, CancellationToken cancellationToken);

    Task<User> CreateUserAsync(User user, CancellationToken cancellationToken);

    Task<bool> DeleteUserByIdAsync(UserId id, CancellationToken cancellationToken);
}
