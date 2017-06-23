using Microsoft.Extensions.Options;
using MongoDB.Driver;


namespace AspNetCore.Identity.MongoDB
{

    public class MongoDbContext<TUser, TRole> : IMongoDBDbContext<TUser, TRole> 
        where TUser : IdentityUser
        where TRole : IdentityRole
    {
        public MongoDbContext(IMongoDatabase database, IOptions<MongoDBOption> option)
        {
            User = database.GetCollection<TUser>(option.Value.User.CollectionName);
            Role = database.GetCollection<TRole>(option.Value.Role.CollectionName);
        }

        public IMongoCollection<TUser> User { get; private set; }
        public IMongoCollection<TRole> Role { get; private set; }
    }
}
