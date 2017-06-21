using System;
using AspNetCore.Identity.MongoDB;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection ConfigureMongoDbOption(this IServiceCollection services, Action<MongoDBOption> configure)
        {
            services.Configure(configure);

            return services;
        }

        public static IServiceCollection AddMongoDatabase<TUser>(this IServiceCollection services)
           where TUser : IdentityUser
        {
            services.AddTransient(provider =>
             {
                 var options = provider.GetService<IOptions<MongoDBOption>>();
                 var client = new MongoClient(options.Value.ConnectionString);
                 var database = client.GetDatabase(options.Value.Database);

                 return database;
             });

            return services;
        }

        public static IServiceCollection AddMongoDbContext<TUser>(this IServiceCollection services) 
            where TUser : IdentityUser
        {
            services.AddTransient<IUserDbContext<TUser>, MongoDbContext<TUser>>();
            return services;
        }

        public static IServiceCollection AddMongoUserStore<TUser>(this IServiceCollection services) 
            where TUser : IdentityUser
        {
            services.AddTransient<IUserStore<TUser>, UserStore<TUser>>();

            return services;
        }
    }
}
