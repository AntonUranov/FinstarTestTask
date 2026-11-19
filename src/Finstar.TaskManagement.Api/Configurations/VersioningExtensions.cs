using Asp.Versioning;

namespace Finstar.TaskManagement.Api.Configurations;

public static class VersioningExtensions
{
    public static IApiVersioningBuilder AddVersioning(this WebApplicationBuilder builder) =>
        builder.Services.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();
                })
            .AddMvc()
            .AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });
}