using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using System;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Api.Service
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
			try
			{
				IConfiguration config = new ConfigurationBuilder()
					.AddJsonFile("appsettings.json")
					.AddEnvironmentVariables()
					.Build();
				var mcConfig = config.GetSection("MultiChainConfiguration").Get<MultiChainConfiguration>();
				logger.Info($"MultiChainDotNet API Config: {mcConfig.ToJson()}");
				CreateHostBuilder(args).Build().Run();
			}
			catch (Exception exception)
			{
				//NLog: catch setup errors
				logger.Error(exception, "Stopped program because of exception");
				throw;
			}
			finally
			{
				// Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
				NLog.LogManager.Shutdown();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(builder =>
				{
					builder
						.UseStartup<Startup>()
						.ConfigureLogging((hostingContext, logging) =>
						{
							logging.ClearProviders();
							NLog.LogManager.Configuration = new NLogLoggingConfiguration(hostingContext.Configuration.GetSection("NLog"));
						})
						.UseNLog()
						.ConfigureAppConfiguration((hostingContext, builder) =>
						{
							builder
								.AddEnvironmentVariables("MULTICHAINDOTNET_")
								;
						})
						;
				});
	}
}
