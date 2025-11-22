using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneTime.Core.Models;
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
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationsTesting");

            builder.ConfigureServices(services =>
            {
                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var context = scopedServices.GetRequiredService<OneTimeContext>();

                // Resets the database to ensure a clean state for each test run
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Adds data to the in-memory database for testing
                SeedTestData(context);
            });
        }

        private void SeedTestData(OneTimeContext context)
        {
            context.Projects.AddRange(
                new Project { ProjectId = 1, Name = "Test Project 1" },
                new Project { ProjectId = 2, Name = "Test Project 2" }
            );

            context.SaveChanges();
        }
    }
}
