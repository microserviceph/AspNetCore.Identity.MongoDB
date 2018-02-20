using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Identity.MongoDB
{
    public class UserStore<TUser, TRole> :
        IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IUserLockoutStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IQueryableUserStore<TUser>,
        IUserAuthenticationTokenStore<TUser>,
        IUserTwoFactorStore<TUser>,
        IUserRoleStore<TUser>,
        IUserStore<TUser>

        where TUser : IdentityUser
        where TRole : IdentityRole

    {
        private readonly IMongoDBDbContext<TUser, TRole> _dbContext;

        public UserStore(IMongoDBDbContext<TUser, TRole> dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public IQueryable<TUser> Users
        {
            get
            {
                return _dbContext.User.AsQueryable();
            }
        }

        public Task AddClaimsAsync(TUser user, IEnumerable<System.Security.Claims.Claim> claims, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.AddClaims(claims.Select(c => new UserClaim
            {
                Type = c.Type,
                Value = c.Value
            })), cancellationToken);
        }

        public Task AddLoginAsync(TUser user, Microsoft.AspNetCore.Identity.UserLoginInfo login, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.AddLogin(new UserLoginInfo
            {
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName,
                ProviderKey = login.ProviderKey,
            }), cancellationToken);
        }

        public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var roleEntity = await _dbContext.Role.Find(r => r.NormalizedName == roleName).SingleOrDefaultAsync(cancellationToken);

            if (roleEntity == null)
            {
                throw new InvalidOperationException($"Role '${roleName}' not found");
            }

            user.AddRole(roleName);
        }

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            await _dbContext.User.InsertOneAsync(user, null, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            await _dbContext.User.DeleteOneAsync(u => u.Id == user.Id, cancellationToken);
            return IdentityResult.Success;
        }

        public void Dispose()
        {
        }

        public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var filter = Builders<TUser>.Filter.Eq(u => u.NormalizedEmail, normalizedEmail);

            return _dbContext.User.Find(filter).SingleOrDefaultAsync(cancellationToken);
        }

        public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var filter = Builders<TUser>.Filter.Eq(u => u.Id, userId);

            return _dbContext.User.Find(filter).SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var found = await _dbContext.User.Find(user => user.Logins.Any(loginInfo => loginInfo.ProviderKey == providerKey), new FindOptions
            {
            }).SingleOrDefaultAsync(cancellationToken);

            return found;
        }

        public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var filter = Builders<TUser>.Filter.Eq(u => u.NormalizedName, normalizedUserName);

            return _dbContext.User.Find(filter).SingleOrDefaultAsync(cancellationToken);
        }

        public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Claims.Select(c => new System.Security.Claims.Claim(c.Type, c.Value)).ToList() as IList<System.Security.Claims.Claim>);
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEndDate);
        }

        public Task<IList<Microsoft.AspNetCore.Identity.UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            var list = user.Logins
                .Select(c => new Microsoft.AspNetCore.Identity.UserLoginInfo(c.LoginProvider, c.ProviderKey, c.ProviderDisplayName))
                .ToList();

            return Task.FromResult<IList<Microsoft.AspNetCore.Identity.UserLoginInfo>>(list);
        }

        public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedName);
        }

        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<string>>(user.Roles);
        }

        public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task<string> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var authToken = user.AuthTokens.SingleOrDefault(t => t.LoginProvider == loginProvider && t.Name == name);

            return Task.FromResult(authToken?.Token);
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public async Task<IList<TUser>> GetUsersForClaimAsync(System.Security.Claims.Claim claim, CancellationToken cancellationToken)
        {
            Expression<Func<TUser, bool>> filter = u => u.Claims.Any(c => c.Type == claim.Type && c.Value == claim.Value);

            return await _dbContext.User.Find(filter)
                            .ToListAsync(cancellationToken);
        }

        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            Expression<Func<TUser, bool>> filter = u => u.Roles.Contains(roleName);

            return await _dbContext.User.Find(filter)
                          .ToListAsync(cancellationToken);
        }

        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.AccessFailedCount++, cancellationToken);
        }

        public Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.Roles.Contains(roleName), cancellationToken);
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable<System.Security.Claims.Claim> claims, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.RemoveClaims(claims.Select(c => new UserClaim
            {
                Type = c.Type,
                Value = c.Value
            })), cancellationToken);
        }

        public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.RemoveRole(roleName), cancellationToken);
        }

        public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.RemoveLogin(loginProvider, providerKey), cancellationToken);
        }

        public Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.RemoveToken(loginProvider, name), cancellationToken);
        }

        public Task ReplaceClaimAsync(TUser user, System.Security.Claims.Claim claim, System.Security.Claims.Claim newClaim, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.ReplaceClaim(new UserClaim(claim), new UserClaim(newClaim)), cancellationToken);
        }

        public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.AccessFailedCount = 0, cancellationToken);
        }

        public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.Email = email, cancellationToken);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.EmailConfirmed = confirmed, cancellationToken);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.LockoutEnabled = enabled, cancellationToken);
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.LockoutEndDate = lockoutEnd, cancellationToken);
        }

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.NormalizedEmail = normalizedEmail, cancellationToken);
        }

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.NormalizedName = normalizedName, cancellationToken);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.PasswordHash = passwordHash, cancellationToken);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.PhoneNumber = phoneNumber, cancellationToken);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.PhoneNumberConfirmed = confirmed, cancellationToken);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.SecurityStamp = stamp, cancellationToken);
        }

        public Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var authToken = user.AuthTokens.SingleOrDefault(t => t.LoginProvider == loginProvider && t.Name == name);
                if (authToken == null)
                    user.AddToken(new AuthToken
                    {
                        Token = value,
                        Name = name,
                        LoginProvider = loginProvider
                    });
                else
                    authToken.Token = value;
            }, cancellationToken);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.TwoFactorEnabled = enabled, cancellationToken);
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.UserName = userName, cancellationToken);
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            await _dbContext.User.FindOneAndReplaceAsync(u => u.Id == user.Id, user, null, cancellationToken);
            return IdentityResult.Success;
        }
    }
}