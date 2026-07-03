using Scalar.AspNetCore;
using StackExchange.Redis;
using WikiGuessrAPI.Infrastructure;
using WikiGuessrAPI.Services;
using WikiGuessrAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
string redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? string.Empty;
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

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
builder.Services.AddScoped<IManageCachedSessionInfo, GameSessionCacher>();
builder.Services.AddScoped<IManageGameSessions, GameSessionManager>();
builder.Services.AddScoped<ICreateAndFetchQuestionListQuestions, QuestionListManager>();
builder.Services.AddScoped<IFetchAnswers, AnswerFetcher>();

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

app.MapControllers();

app.Run();
