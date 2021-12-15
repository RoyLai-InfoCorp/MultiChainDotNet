using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MultiChainDotNet.Api.Service
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging(logging =>
				{
					logging.ClearProviders();
					logging.AddConsole();
				})
				.ConfigureAppConfiguration((hostingContext, builder) =>
				{
					builder
						.AddEnvironmentVariables("MULTICHAINDOTNET_")
						;
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
