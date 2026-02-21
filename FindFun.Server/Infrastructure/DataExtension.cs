using Azure.Storage.Blobs;
using FindFun.Server.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FindFun.Server.Infrastructure;

public static class DataExtension
{
    public static async Task InitializeDbAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FindFunDbContext>();
        await context.Database.MigrateAsync();
    }

    public static WebApplicationBuilder AddConnectionStrings(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<ConnectionStrings>()
         .Bind(builder.Configuration.GetSection(nameof(ConnectionStrings)))
         .ValidateOnStart()
         .ValidateDataAnnotations();
        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<FindFunDbContext>((serviceProvider, options) =>
        {
            var dbOptions = serviceProvider.GetRequiredService<IOptions<ConnectionStrings>>().Value;
            var connectionString = dbOptions?.FindFun;
            options.UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite());
        });

        return builder;
    }

    public static WebApplicationBuilder AddBlobStorage(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(sp =>
        {
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var connOptions = sp.GetRequiredService<IOptions<ConnectionStrings>>().Value;
            var connectionString = connOptions?.Blobs
                ?? (env.IsDevelopment() ? "UseDevelopmentStorage=true" : null);

            return new BlobServiceClient(connectionString);
        });

        return builder;
    }
}
