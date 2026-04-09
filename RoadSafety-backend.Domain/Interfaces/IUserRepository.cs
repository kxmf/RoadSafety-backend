using RoadSafety_backend.Domain.Entities;
using RoadSafety_backend.Domain.ValueObjects.IDs;

namespace RoadSafety_backend.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(UserId id, CancellationToken cancellationToken);

    Task<User> CreateUserAsync(User user, CancellationToken cancellationToken);

    Task UpdateUserAsync(User newUser, CancellationToken cancellationToken);

    Task<bool> DeleteUserByIdAsync(UserId id, CancellationToken cancellationToken);
}
