using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Api.Tests.TestHelpers
{
    /// <summary>
    /// Provides a custom web factory for integration testing the OneTime API.
    /// </summary>
    public class OneTimeApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = Guid.NewGuid().ToString();
        
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationsTesting");

            builder.ConfigureServices(services =>
            {
                // Fjern eksisterende DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<OneTimeContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                // Test-database (unik pr. run)
                services.AddDbContext<OneTimeContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });
            });
        }
    }
}
