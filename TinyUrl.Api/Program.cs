using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TinyUrl.Api.AliasKeyService;
using TinyUrl.Data;
using TinyUrl.Data.Models;
using TinyUrl.Data.Redis;
using TinyUrl.Data.Repositories;
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;

namespace TinyUrl.Api
{
    public class Program
    {
        public static CommandLineOptions CommandLineArguments { get; private set; }

        public static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
                       .WithParsed<CommandLineOptions>(opts => CommandLineArguments = opts)
                       .WithNotParsed<CommandLineOptions>((errs) => Console.Error.WriteLine("Unable to parse command line options"));

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel()
                              .ConfigureAppConfiguration((context, config) =>
                              {
                                  IHostEnvironment env = context.HostingEnvironment;

                                  config.SetBasePath(env.ContentRootPath)
                                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false)
                                        .AddJsonFile($"appsettings.{env.EnvironmentName}.secrets.json", optional: true, reloadOnChange: false)
                                        .AddJsonFile(Path.Combine("config", $"appsettings.json"), optional: true, reloadOnChange: false)
                                        .AddJsonFile(Path.Combine("config", $"appsettings.{env.EnvironmentName}.json"), optional: true, reloadOnChange: false)
                                        .AddJsonFile(Path.Combine("secrets", $"appsettings.secrets.json"), optional: true, reloadOnChange: false)
                                        .AddJsonFile(Path.Combine("secrets", $"appsettings.{env.EnvironmentName}.secrets.json"), optional: true, reloadOnChange: false)
                                        .AddEnvironmentVariables();
                              })
                              .UseDefaultServiceProvider((context, options) =>
                              {
                                  options.ValidateScopes = context.HostingEnvironment.EnvironmentName.Contains("development", StringComparison.CurrentCultureIgnoreCase);
                              })
                              .ConfigureLogging((context, logging) =>
                              {
                                  logging.AddDebug();
                              })
                              .ConfigureServices((context, services) =>
                              {
                                  IConfiguration configuration = context.Configuration;

                                  services.AddControllers();

                                  services.AddSingleton<AliasKeyService.AliasKeyService>();
                                  services.AddSingleton<ILinkCacheProvider>(new RedisLinkCacheProvider(configuration.GetConnectionString("Redis")));

                                  services.AddDbContext<TinyUrlDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("SqlServer"), o => o.MigrationsAssembly("TinyUrl.Data.SqlServer")));
                                  services.AddScoped<IUnitOfWork, UnitOfWork>();
                              })
                              .Configure(async (app) =>
                              {
                                  IHostEnvironment context = app.ApplicationServices.GetService<IHostEnvironment>();

                                  if (CommandLineArguments.SeedKeys)
                                  {
                                      AliasKeyService.AliasKeyService keyService = app.ApplicationServices.GetService<AliasKeyService.AliasKeyService>();
                                      await keyService.SeedKeys(CommandLineArguments.KeyLength);

                                      Console.WriteLine("Alias keys have been seeded.");

                                      IApplicationLifetime applicationLifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
                                      applicationLifetime.StopApplication();
                                  }

                                  app.UseForwardedHeaders(new ForwardedHeadersOptions
                                  {
                                      ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                                      RequireHeaderSymmetry = false
                                  });

                                  app.UseRouting();

                                  app.UseAuthorization();

                                  app.UseEndpoints(endpoints =>
                                  {
                                      endpoints.MapControllers();
                                  });
                              });
                });

        public class CommandLineOptions
        {
            [Option(Default = false, HelpText = "Seed keys.")]
            public bool SeedKeys { get; set; }

            [Option(Default = 3, HelpText = "Key length.")]
            public int KeyLength { get; set; }
        }
    }
}
