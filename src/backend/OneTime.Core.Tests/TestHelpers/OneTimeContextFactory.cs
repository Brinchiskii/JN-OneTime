using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OneTime.Core.Models;

namespace OneTime.Core.Tests.TestHelpers
{
    public static class OneTimeContextFactory
    {
        public static OneTimeContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<OneTimeContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new OneTimeContext(options);
        }
    }
}
