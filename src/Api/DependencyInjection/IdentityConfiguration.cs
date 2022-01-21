using Api.Data;
using Microsoft.AspNetCore.Identity;

namespace Api.DependencyInjection
{
    public static class IdentityConfiguration
    {
        public static IServiceCollection AddIdentityConfig(this IServiceCollection services)
        {
            services.AddIdentity<Microsoft.AspNetCore.Identity.IdentityUser, IdentityRole>()
                        .AddEntityFrameworkStores<ApiDbContext>()
                        .AddDefaultTokenProviders();

            return services;
        }
    }
}