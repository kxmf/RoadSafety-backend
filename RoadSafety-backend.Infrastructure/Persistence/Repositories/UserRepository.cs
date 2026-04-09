using Microsoft.EntityFrameworkCore;
using RoadSafety_backend.Domain.Entities;
using RoadSafety_backend.Domain.Interfaces;
using RoadSafety_backend.Domain.ValueObjects.IDs;
using RoadSafety_backend.Infrastructure.Persistence.Context;

namespace RoadSafety_backend.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(UserDbContext dbContext) : IUserRepository
{
    private readonly UserDbContext _dbContext = dbContext;

    public async Task<User> CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<bool> DeleteUserByIdAsync(UserId id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
            return false;

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<User?> GetUserByIdAsync(UserId id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateUserAsync(User newUser, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == newUser.Id, cancellationToken);


        if (user is null)
        {
            // TODO: добавить какой-то exception
        }
        else
            user = newUser;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
