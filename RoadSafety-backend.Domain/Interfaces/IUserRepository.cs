using RoadSafety_backend.Domain.Entities;
using RoadSafety_backend.Domain.ValueObjects.IDs;

namespace RoadSafety_backend.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(UserId id, CancellationToken cancellationToken = default);

    Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default);

    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default);

    Task<User> DeleteUserAsync(UserId id, CancellationToken cancellationToken = default);
}
