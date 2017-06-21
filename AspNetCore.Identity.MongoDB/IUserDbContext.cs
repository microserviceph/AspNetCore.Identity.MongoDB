using MongoDB.Driver;


namespace AspNetCore.Identity.MongoDB
{
    public interface IUserDbContext<TUser>
    {
        IMongoCollection<TUser> User { get; }
    }

    public interface IRoleDbContext<TRole>
    {
        IMongoCollection<TRole> Role { get; }
    }
}