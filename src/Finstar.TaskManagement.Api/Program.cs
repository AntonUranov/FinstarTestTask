using System.Net.Mime;
using Finstar.TaskManagement.Api;
using Finstar.TaskManagement.Api.Configurations;
using Finstar.TaskManagement.Api.Controllers.Tasks;
using Finstar.TaskManagement.Api.Middlewares;
using Finstar.TaskManagement.Api.Options;
using Finstar.TaskManagement.Api.Toolkit;
using Finstar.TaskManagement.Dal;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.SetupFinstarOptions())
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
            new BadRequestObjectResult(context.ModelState)
            {
                ContentTypes = { MediaTypeNames.Application.Json }
            };
    });

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.RegisterOptions<DbExecutionStrategyOptions>();

builder.Services.AddDbContext<TaskManagementContext>( (providers, options) =>
    {
        var dbExecutionStrategyOptions = providers.
            GetRequiredService<IOptions<DbExecutionStrategyOptions>>().Value;
        options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(TaskManagementContext)),
                npgSqlOptions =>
                {
                    npgSqlOptions.MigrationsAssembly("Finstar.TaskManagement.Dal");
                    npgSqlOptions.EnableRetryOnFailure(dbExecutionStrategyOptions.MaxRetryCount,
                        TimeSpan.FromMinutes(dbExecutionStrategyOptions.MaxDelaySeconds), null);
                })
            .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
            .EnableDetailedErrors();
    }
);
builder.Services.AddScoped<ITaskManagementStorage, TaskManagementContext>(sp => sp.GetRequiredService<TaskManagementContext>());
builder.Services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddSingleton(TimeProvider.System);

builder.AddVersioning();

builder.AddObservability();

builder.AddLogging();

builder.Services.RegisterOptions<RabbitMqOptions>();

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<TaskManagementContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
        o.QueryDelay = TimeSpan.FromSeconds(1);
    });
    
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqOptions = context.GetService<IOptions<RabbitMqOptions>>()?.Value ?? throw new ConfigurationException("RabbitMqOptions not found");
        
        cfg.Host(
            rabbitMqOptions.Host,
            rabbitMqOptions.Port,
            rabbitMqOptions.VirtualHost,
            h =>
            {
                h.Username(rabbitMqOptions.Username);
                h.Password(rabbitMqOptions.Password);
            }
        );
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddSingleton<ITaskItemsMapper, TaskItemsMapper>();

builder.Services.AddScoped<IUserSeedService, UserSeedService>();

builder.Services.AddAllElasticApm();

var app = builder.Build();

app. UseMiddleware<GlobalExceptionHandlerMiddleware>()
    .UseHttpsRedirection()
    .UseResponseCompression()
    .UseRouting()
    .UseEndpoints(endpoints => endpoints.MapControllers());

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "TaskManagement API");
    });
}

await app.Services.MigrateDatabase();

app.Run();
