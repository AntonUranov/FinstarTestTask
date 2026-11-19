using System.ComponentModel.DataAnnotations;

namespace Finstar.TaskManagement.Api.Options;

public class RabbitMqOptions
{
    [Required]
    public string Host { get; init; }

    [Required]
    public string VirtualHost { get; init; }

    [Required]
    public ushort Port { get; init; }

    [Required]
    public string Username { get; init; }

    [Required]
    public string Password { get; init; }
}