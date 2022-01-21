namespace Api.DependencyInjection
{
    public static class RedisConfiguration
    {
        public static IServiceCollection AddRedisConfig(this IServiceCollection services)
        {
             return services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = "localhost:6379";
                    });
        }
    }
}