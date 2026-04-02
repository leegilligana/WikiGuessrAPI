using Scalar.AspNetCore;
using StackExchange.Redis;
using WikiGuessrAPI.Services;
using WikiGuessrAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
string redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? string.Empty;
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "WikiGuessr_";
});
builder.Services.AddOpenApi();

// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton<IWrapDapper>(new DapperWrapper(string.Empty));
builder.Services.AddScoped<IRedisCache, RedisCache>();
builder.Services.AddScoped<IManageGameSessions, GameSessionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
