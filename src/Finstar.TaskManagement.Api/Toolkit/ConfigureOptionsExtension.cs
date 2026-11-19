namespace Finstar.TaskManagement.Api.Toolkit;

public static class ConfigureOptionsExtension
{
    public static IServiceCollection RegisterOptions<T>(
        this IServiceCollection services,
        string? configSectionPath = null,
        bool validateDataAnnotations = true
    ) where T : class
    {
        var optionsBuilder = services.AddOptions<T>()
            .BindConfiguration(configSectionPath ?? typeof(T).Name);

        if (validateDataAnnotations)
        {
            optionsBuilder.ValidateDataAnnotations();
            optionsBuilder.ValidateOnStart();
        }

        return services;
    }
}