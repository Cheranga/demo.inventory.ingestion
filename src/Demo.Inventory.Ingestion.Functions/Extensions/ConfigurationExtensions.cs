using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Inventory.Ingestion.Functions.Extensions;

[ExcludeFromCodeCoverage]
public static class ConfigurationExtensions
{
    public static void RegisterFromConfiguration<TConfiguration>(
        this IServiceCollection services,
        IConfiguration configuration,
        ServiceLifetime lifeTime = ServiceLifetime.Transient
    )
    {
        var settings = configuration.GetSection(typeof(TConfiguration).Name).Get<TConfiguration>();
        services.Add(new ServiceDescriptor(typeof(TConfiguration), _ => settings, lifeTime));
    }
}