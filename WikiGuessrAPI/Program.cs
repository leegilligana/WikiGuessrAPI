using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Scalar.AspNetCore;
using StackExchange.Redis;
using WikiGuessrAPI;
using WikiGuessrAPI.Services;
using WikiGuessrAPI.Services.Classes;
using WikiGuessrAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
string redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? string.Empty;

var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

var multiplexers = new List<RedLockMultiplexer>
{
    new(redisConnection),
};

var redlockFactory = RedLockFactory.Create(multiplexers);
builder.Services.AddSingleton(redlockFactory);
builder.Services.AddHostedService<GameSessionOrchestrator>();

builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnectionString, options =>
    {
        options.Configuration.ChannelPrefix = "WikiGuessr";
    });

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        var exception = context.HttpContext.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?
            .Error;

        if (exception != null)
        {
            context.ProblemDetails.Detail = exception.Message;
            context.ProblemDetails.Extensions["exceptionType"] = exception.GetType().Name;
        }
    };
});

// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton<IWrapDapper>(new DapperWrapper(string.Empty));
builder.Services.AddScoped<IManageInactiveSessionCache, InactiveSessionCacher>();
builder.Services.AddScoped<IManageInactiveSessions, InactiveSessionManager>();
builder.Services.AddScoped<IManageActiveSessions, ActiveSessionManager>();
builder.Services.AddScoped<IManageActiveSessionCache, ActiveSessionCacher>();
builder.Services.AddScoped<IManageRoundInfo, RoundInfoManager>();
builder.Services.AddScoped<IFetchAnswers, AnswerFetcher>();
builder.Services.AddScoped<IDoGameTicks, GameTickProcessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthorization();
app.MapHub<GameSessionHub>("/gameSessionHub");

app.MapControllers();

app.Run();
