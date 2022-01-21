using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.DependencyInjection
{
     public static class EntityFrameworkConfiguration
    {
        private const string connectionString = "Data Source=db;Initial Catalog=IdentityDb;User Id=sa;Password=Test@123;Trusted_Connection=false";
        public static IServiceCollection AddEFConfig(this IServiceCollection services)
        {
             return services.AddDbContext<ApiDbContext>(options => options
                            .UseSqlServer(connectionString));
        }
    }
}