using MongoDB.Driver;


namespace AspNetCore.Identity.MongoDB
{
    public interface IMongoDBDbContext<TUser, TRole>
    {
        IMongoCollection<TUser> User { get; }

        IMongoCollection<TRole> Role { get; }
    }
}