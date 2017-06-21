using Microsoft.Extensions.Options;
using MongoDB.Driver;


namespace AspNetCore.Identity.MongoDB
{

    public class MongoDbContext<TUser> : IUserDbContext<TUser> where TUser : IdentityUser
    {
        public MongoDbContext(IMongoDatabase database, IOptions<MongoDBOption> option)
        {
            User = database.GetCollection<TUser>(option.Value.User.CollectionName);
        }

        public IMongoCollection<TUser> User { get; private set; }
    }
}