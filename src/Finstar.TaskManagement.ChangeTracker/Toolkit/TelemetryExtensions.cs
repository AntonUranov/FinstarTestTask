using System.Reflection;
using MassTransit.Logging;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Finstar.TaskManagement.ChangeTracker;

public static class TelemetryExtensions
{
    private static readonly AssemblyName AssemblyName = Assembly.GetExecutingAssembly().GetName();
    
    public static OpenTelemetryBuilder AddObservability(this WebApplicationBuilder builder) =>
        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(
                    serviceName: AssemblyName.Name!,
                    serviceVersion: AssemblyName.Version?.ToString(),
                    serviceInstanceId: Environment.MachineName);
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddNpgsql()
                    .AddSource(DiagnosticHeaders.DefaultListenerName);
                
                tracing.AddConsoleExporter();
            });
    
    public static ILoggingBuilder AddLogging(this WebApplicationBuilder builder) =>
        builder.Logging.AddOpenTelemetry(o =>
        {
            o.IncludeFormattedMessage = true;
            o.IncludeScopes = true;
            o.AddConsoleExporter();
        });
}