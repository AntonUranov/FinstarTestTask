using System.Net.Mime;
using Finstar.TaskManagement.Api.Toolkit;
using Finstar.TaskManagement.ChangeTracker;
using Finstar.TaskManagement.ChangeTracker.Dal;
using Finstar.TaskManagement.ChangeTracker.Middlewares;
using Finstar.TaskManagement.ChangeTracker.Options;
using Finstar.TaskManagement.ChangeTracker.Toolkit;
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

builder.Services.AddDbContext<TaskChangesContext>( (providers, options) =>
    {
        var dbExecutionStrategyOptions = providers.
            GetRequiredService<IOptions<DbExecutionStrategyOptions>>().Value;
        options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(TaskChangesContext)),
                npgSqlOptions =>
                {
                    npgSqlOptions.MigrationsAssembly("Finstar.TaskManagement.ChangeTracker");
                    npgSqlOptions.EnableRetryOnFailure(dbExecutionStrategyOptions.MaxRetryCount,
                        TimeSpan.FromMinutes(dbExecutionStrategyOptions.MaxDelaySeconds), null);
                })
            .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
            .EnableDetailedErrors();
    }
);
builder.Services.AddScoped<ITaskChangesStorage, TaskChangesContext>();

builder.AddObservability();

builder.AddLogging();

builder.Services.RegisterOptions<RabbitMqOptions>();

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, cfg) =>
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
    
    configurator.AddConsumer<TaskChangedEventConsumer>();
});

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
        options.SwaggerEndpoint("/openapi/v1.json", "ChangeTracker API");
    });
}

await app.Services.MigrateDatabase();

app.Run();
