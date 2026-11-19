using System.Text.Json;
using System.Text.Json.Serialization;

namespace Finstar.TaskManagement.Api.Toolkit;

public static class Serialization
{
    public static readonly JsonSerializerOptions DefaultOptions = CreateDefaultOptions();

    public static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions();
        options.SetupFinstarOptions();
        return options;
    }

    public static void SetupFinstarOptions(this JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter());
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
}