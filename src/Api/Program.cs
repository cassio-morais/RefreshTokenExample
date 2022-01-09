using IdentityUser.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt => opt.SwaggerDoc("v1", new OpenApiInfo { Title = "RefreshTokenExample", Version = "v1" }));

// database ef context
builder.Services
    .AddDbContext<ApiDbContext>(options => options
    .UseSqlServer("Data Source=localhost,51433;Initial Catalog=IdentityDb;User Id=sa;Password=Strong!Password"));

// identity
builder.Services.AddIdentity<Microsoft.AspNetCore.Identity.IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApiDbContext>()
    .AddDefaultTokenProviders();

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
