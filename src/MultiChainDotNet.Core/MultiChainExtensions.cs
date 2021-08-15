using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainBlockchain;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Core.MultiChainVariable;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core
{
    public static class MultiChainExtensions
    {
		static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
		{
			return HttpPolicyExtensions
				.HandleTransientHttpError()
				.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
																			retryAttempt)));
		}

		static IServiceCollection AddMultiChainAddress(this IServiceCollection services, MultiChainConfiguration mcConfig)
		{
			services
				.AddScoped<MultiChainAddressCommand>()
				.AddHttpClient<MultiChainAddressCommand>(c => c.BaseAddress = new Uri($"http://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}"))
					.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
					.AddPolicyHandler(GetRetryPolicy())
					.ConfigurePrimaryHttpMessageHandler(() => {
						return new HttpClientHandler()
						{
							Credentials = new NetworkCredential(mcConfig.Node.RpcUserName, mcConfig.Node.RpcPassword)
						};
					});

			return services;
		}

		static IServiceCollection AddMultiChainTransaction(this IServiceCollection services, MultiChainConfiguration mcConfig)
		{
			services
				.AddScoped<MultiChainTransactionCommand>()
				.AddHttpClient<MultiChainTransactionCommand>(c => c.BaseAddress = new Uri($"http://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}"))
					.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
					.AddPolicyHandler(GetRetryPolicy())
					.ConfigurePrimaryHttpMessageHandler(() =>
					{
						return new HttpClientHandler()
						{
							Credentials = new NetworkCredential(mcConfig.Node.RpcUserName, mcConfig.Node.RpcPassword)
						};
					});

			return services;
		}

		static IServiceCollection AddMultiChainAsset(this IServiceCollection services, MultiChainConfiguration mcConfig)
		{
			services
				.AddScoped<MultiChainAssetCommand>()
				.AddHttpClient<MultiChainAssetCommand>(c => c.BaseAddress = new Uri($"http://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}"))
					.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
					.AddPolicyHandler(GetRetryPolicy())
					.ConfigurePrimaryHttpMessageHandler(() =>
					{
						return new HttpClientHandler()
						{
							Credentials = new NetworkCredential(mcConfig.Node.RpcUserName, mcConfig.Node.RpcPassword)
						};
					});

			return services;
		}

		static IServiceCollection AddMultiChainStream(this IServiceCollection services, MultiChainConfiguration mcConfig)
		{
			services
				.AddScoped<MultiChainStreamCommand>()
				.AddHttpClient<MultiChainStreamCommand>(c => c.BaseAddress = new Uri($"http://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}"))
					.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
					.AddPolicyHandler(GetRetryPolicy())
					.ConfigurePrimaryHttpMessageHandler(() =>
					{
						return new HttpClientHandler()
						{
							Credentials = new NetworkCredential(mcConfig.Node.RpcUserName, mcConfig.Node.RpcPassword)
						};
					});

			return services;
		}

		static IServiceCollection AddMultiChainVariable(this IServiceCollection services, MultiChainConfiguration mcConfig)
		{
			services
				.AddScoped<MultiChainVariableCommand>()
				.AddHttpClient<MultiChainVariableCommand>(c => c.BaseAddress = new Uri($"http://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}"))
					.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
					.AddPolicyHandler(GetRetryPolicy())
					.ConfigurePrimaryHttpMessageHandler(() =>
					{
						return new HttpClientHandler()
						{
							Credentials = new NetworkCredential(mcConfig.Node.RpcUserName, mcConfig.Node.RpcPassword)
						};
					});

			return services;
		}


		static IServiceCollection AddMultiChainPermission(this IServiceCollection services, MultiChainConfiguration mcConfig)
		{
			services
				.AddScoped<MultiChainPermissionCommand>()
				.AddHttpClient<MultiChainPermissionCommand>(c => c.BaseAddress = new Uri($"http://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}"))
					.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
					.AddPolicyHandler(GetRetryPolicy())
					.ConfigurePrimaryHttpMessageHandler(() =>
					{
						return new HttpClientHandler()
						{
							Credentials = new NetworkCredential(mcConfig.Node.RpcUserName, mcConfig.Node.RpcPassword)
						};
					});

			return services;
		}

		static IServiceCollection AddMultiChainBlockchain(this IServiceCollection services, MultiChainConfiguration mcConfig)
		{
			services
				.AddScoped<MultiChainBlockchainCommand>()
				.AddHttpClient<MultiChainBlockchainCommand>(c => c.BaseAddress = new Uri($"http://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}"))
					.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
					.AddPolicyHandler(GetRetryPolicy())
					.ConfigurePrimaryHttpMessageHandler(() =>
					{
						return new HttpClientHandler()
						{
							Credentials = new NetworkCredential(mcConfig.Node.RpcUserName, mcConfig.Node.RpcPassword)
						};
					});

			return services;
		}

		static IServiceCollection AddMultiChainCommandFactory(this IServiceCollection services, MultiChainConfiguration mcConfig)
		{
			services
				.AddSingleton<IMultiChainCommandFactory, MultiChainCommandFactory>()
				.AddHttpClient<IMultiChainCommandFactory, MultiChainCommandFactory>(c => c.BaseAddress = new Uri($"http://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}"))
					.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
					.AddPolicyHandler(GetRetryPolicy())
					.ConfigurePrimaryHttpMessageHandler(() =>
					{
						return new HttpClientHandler()
						{
							Credentials = new NetworkCredential(mcConfig.Node.RpcUserName, mcConfig.Node.RpcPassword)
						};
					});

			return services;
		}


		public static IServiceCollection AddMultiChain(this IServiceCollection services)
		{
			var mcConfig = services.BuildServiceProvider().GetRequiredService<MultiChainConfiguration>();
			services.AddMultiChainAddress(mcConfig);
			services.AddMultiChainTransaction(mcConfig);
			services.AddMultiChainAsset(mcConfig);
			services.AddMultiChainStream(mcConfig);
			services.AddMultiChainPermission(mcConfig);
			services.AddMultiChainBlockchain(mcConfig);
			services.AddMultiChainVariable(mcConfig);
			services.AddMultiChainCommandFactory(mcConfig);
			return services;
		}
	}
}
