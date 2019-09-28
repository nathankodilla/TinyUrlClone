using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TinyUrl.Data.Models;

namespace TinyUrl.Data.SqlServer
{
    public class TinyUrlDbContextFactory : IDesignTimeDbContextFactory<TinyUrlDbContext>
    {
        public TinyUrlDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                        .AddJsonFile("appsettings.json", optional: true)
                                        .AddJsonFile("appsettings.Development.json", optional: true)
                                        .Build();

            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(config.GetConnectionString("SqlServer"), o => o.MigrationsAssembly("TinyUrl.Data.SqlServer"));
            return new TinyUrlDbContext(optionsBuilder.Options);
        }
    }
}
