using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Api.DependencyInjection
{
    public static class AuthenticationConfiguration
    {
        public static IServiceCollection AddAuthenticationConfig(this IServiceCollection services)
        {
            var byteArraySecretKey = Encoding.ASCII.GetBytes("#GREATSUPERSECRET#");

            services.AddAuthentication(authOptions =>
                {
                    authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; 
                    authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; 

                }).AddJwtBearer(jwtBearerOptions =>
                {
                    jwtBearerOptions.RequireHttpsMetadata = true;
                    jwtBearerOptions.SaveToken = true;

                    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(byteArraySecretKey),
                        ValidateAudience = false,
                        ValidateIssuer = false,
                    };
                });

            return services;
        }
    }
}