using Azure.Storage.Blobs;
using Bogus;
using FindFun.Server;
using FindFun.Server.Domain;
using FindFun.Server.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;

namespace FindFund.Server.IntegrationTest;

public class WebAplicationCustomFactory : WebApplicationFactory<IServerMaker>, IAsyncLifetime
{
    public string MunicipalityName { get; private set; } = null!;
    private readonly Faker<Municipality> _faker;
    private IServiceScope? _scope;
    private FindFunDbContext? _dbContext;
    private readonly PostgreSqlContainer postgresContainer = new PostgreSqlBuilder("postgres:16")
              .WithImage("postgis/postgis:15-3.4")
              .Build();
    private readonly AzuriteContainer azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite").Build();
    public WebAplicationCustomFactory()
    {
        var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        _faker = new Faker<Municipality>()
               .CustomInstantiator(f => Municipality.Create(
                   f.Random.Int(1, 1000),
                   f.Address.ZipCode(),
                   f.Address.CountryCode(),
                   f.Address.StateAbbr(),
                   f.Address.City(),
                   f.Address.StreetAddress(),
                   f.Address.SecondaryAddress(),
                   f.Random.AlphaNumeric(10),
                   f.Address.CountryCode(),
                   "type",
                   "local",
                   geometryFactory.CreateMultiPolygon(
                   [
                       geometryFactory.CreatePolygon(
                           geometryFactory.CreateLinearRing(
                           [
                               new Coordinate(-1, -1),
                               new Coordinate(1, -1),
                               new Coordinate(1, 1),
                               new Coordinate(-1, 1),
                               new Coordinate(-1, -1),
                           ])
                       )
                   ])
               ));
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(FindFunDbContext));
            services.AddDbContext<FindFunDbContext>(options =>
            {
                options.UseNpgsql(postgresContainer.GetConnectionString(), npgsqlOptions => npgsqlOptions.UseNetTopologySuite());
            });
            services.RemoveAll(typeof(BlobServiceClient));
            services.AddSingleton(sp => new BlobServiceClient(azuriteContainer.GetConnectionString()));
        });
    }

    public async Task<Park?> GetParkByIdAsync(int id)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FindFunDbContext>();

        var park = await db.Parks
            .Include(p => p.ClosingSchedule)
            .Include(p => p.Images)
            .Include(p => p.Amenities)
                .ThenInclude(pa => pa.Amenity)
            .FirstOrDefaultAsync(p => p.Id == id);

        return park;
    }

    public async Task AddMunicipality()
    {
        var municipality = _faker.Generate();
        _dbContext?.Municipalities.AddAsync(municipality);
        await _dbContext?.SaveChangesAsync()!;
        MunicipalityName = municipality.OfficialNa6;
    }

    public async Task AddParkWithAddressAsync(string municipalityName)
    {
        var municipality = _dbContext?.Municipalities.First(m => m.OfficialNa6 == municipalityName);

        var street = new Street("Main Street", municipality!.Gid);
        await _dbContext!.Streets.AddAsync(street);
        var address = new Address("Some formatted address", "12345", street, -3.70379, 40.41678, "1");
        await _dbContext.Addresses.AddAsync(address);
        var park = new Park("Existing Park", "desc", address, 5.00m, false, "Tester", "Public", "ABC123");
        await _dbContext.Parks.AddAsync(park);
        await _dbContext.SaveChangesAsync();
    }

    public async Task InitializeAsync()
    {
        await postgresContainer.StartAsync();
        await azuriteContainer.StartAsync();
        _scope = Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<FindFunDbContext>();
    }
    async Task IAsyncLifetime.DisposeAsync()
    {
        _scope?.Dispose();
        await azuriteContainer.DisposeAsync();
        await postgresContainer.DisposeAsync();
    }
}