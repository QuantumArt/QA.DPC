using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace QA.ProductCatalog.HighloadFront.Core.API.Configurations;

public static class JsonEnvironmentConfigurationExtensions
{
    public static void LoadConfiguration(this IConfigurationBuilder builder)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var useTemplate = Environment.GetEnvironmentVariable("USE_TEMPLATE") == "1";

        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        if (useTemplate)
        {
            builder.AddJsonEnvironmentFile($"appsettings.{environment}.json");
        }
        else
        {
            builder.AddJsonFile($"appsettings.{environment}.json", optional: true);
        }

        builder.AddEnvironmentVariables();
    }

    public static IConfigurationBuilder AddJsonEnvironmentFile(this IConfigurationBuilder builder, string path)
    {
        return builder.AddJsonEnvironmentFile(provider: null, path: path, optional: false, reloadOnChange: false);
    }

    private static IConfigurationBuilder AddJsonEnvironmentFile(this IConfigurationBuilder builder,
        IFileProvider provider,
        string path, bool optional, bool reloadOnChange)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Не указан путь до файла конфигурации", nameof(path));
        }

        return builder.AddJsonEnvironmentFile(s =>
        {
            s.FileProvider = provider;
            s.Path = path;
            s.Optional = optional;
            s.ReloadOnChange = reloadOnChange;
            s.ResolveFileProvider();
        });
    }

    private static IConfigurationBuilder AddJsonEnvironmentFile(this IConfigurationBuilder builder,
        Action<JsonEnvironmentConfigurationSource> configureSource)
        => builder.Add(configureSource);
}