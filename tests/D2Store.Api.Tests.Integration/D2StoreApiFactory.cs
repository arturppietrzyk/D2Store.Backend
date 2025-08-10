using D2Store.Api.Features.Users.Domain;
using D2Store.Api.Infrastructure;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace D2Store.Api.Tests.Integration;

public class D2StoreApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly TestcontainersContainer _dbContainer = new TestcontainersBuilder<TestcontainersContainer>()
    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
    .WithEnvironment("SA_PASSWORD", "ArturStrong!Passw0rd")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithPortBinding(1433, 1433)
    .WithWaitStrategy(Wait.ForUnixContainer()
    .UntilPortIsAvailable(1433)
    .UntilCommandIsCompleted("/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "ArturStrong!Passw0rd", "-Q", "SELECT 1", "-C"))
    .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }
            var connectionString = "Server=localhost,1433;Database=D2Store;User Id=sa;Password=ArturStrong!Passw0rd;TrustServerCertificate=True;";
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        //await SeedTestDataAsync(context);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    private static async Task SeedTestDataAsync(AppDbContext context)
    {
        if (!await context.Users.AnyAsync())
        {
            var users = new[]
            {
            User.Register(
                firstName: "John",
                lastName: "Doe",
                email: "john.doe@test.com",
                passwordHash: "Password",
                phoneNumber: "+1234567890",
                address: "123 Test Street, Test City"
            )
        };
            context.Users.AddRange(users);
            await context.SaveChangesAsync();
        }
    }
}