using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;

public sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<bool> IsEmailInUseAsync(MailAddress email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Contacts.MailAddress == email, cancellationToken);
    }

    public async Task<bool> IsPhoneInUseAsync(PhoneNumber phone, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Contacts.PhoneNumber!.Value == phone.Value, cancellationToken);
    }

    public async Task<User> CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(user);

        return user;
    }

    public async Task<bool> DeleteUserByIdAsync(UserId id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (user is null)
            return false;

        _dbContext.Users.Remove(user);

        return true;
    }

    public async Task<User?> GetUserByEmailAsync(MailAddress email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Contacts.MailAddress == email, cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(UserId id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<User?> GetUserByPhoneAsync(PhoneNumber phone, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Contacts.PhoneNumber!.Value == phone.Value, cancellationToken);
    }
}
