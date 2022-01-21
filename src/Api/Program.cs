using Api.Data;
using Api.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig();
builder.Services.AddEFConfig();
builder.Services.AddIdentityConfig();
builder.Services.AddRedisConfig();
builder.Services.AddAuthenticationConfig();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
EnsureDatabaseCreated(app);

app.Run();

void EnsureDatabaseCreated(WebApplication app)
{
    var serviceScopeFactory = app.Services.GetService<IServiceScopeFactory>();

    if (serviceScopeFactory is null) throw new NullReferenceException();

    using (var serviceScope = serviceScopeFactory.CreateScope())
    {
        var context = serviceScope.ServiceProvider.GetRequiredService<ApiDbContext>();
        context.Database.Migrate();
    }
}