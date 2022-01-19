using IdentityUser.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.DependencyInjection
{
     public static class EntityFrameworkConfiguration
    {
        private const string connectionString = "Data Source=localhost,51433;Initial Catalog=IdentityDb;User Id=sa;Password=Strong!Password";
        public static IServiceCollection AddEFConfig(this IServiceCollection services)
        {
             return services.AddDbContext<ApiDbContext>(options => options
                            .UseSqlServer(connectionString));
        }
    }
}