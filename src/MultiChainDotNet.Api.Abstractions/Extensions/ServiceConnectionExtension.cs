using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;

namespace MultiChainDotNet.Api.Abstractions.Extensions
{
    public static class ServiceConnectionExtension
    {
		public static IServiceCollection AddChronnetApiClient(this IServiceCollection services)
		{
			// Configure refit for ChronnetApi
			var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
			var host = config.GetSection("MultiChainDotNetApi");
			if (host is { })
			{
				var api = host.Get<string>();
				services
					.AddRefitClient<IJsonRpcApi>()
					.ConfigureHttpClient(c => c.BaseAddress = new Uri(api))
					;
			}
			return services;

		}

	}
}
