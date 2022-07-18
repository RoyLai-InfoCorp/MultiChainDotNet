// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainBinary;
using MultiChainDotNet.Core.MultiChainBlockchain;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.MultiChainToken;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Core.MultiChainVariable;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using MultiChainDotNet.Core.MultiChainFilter;

namespace MultiChainDotNet.Core
{
	public static class MultiChainExtensions
	{
		static TimeSpan HandlerLifeTime = TimeSpan.FromMinutes(5);

		static IServiceCollection ConfigureHttpClient<T>(this IServiceCollection services) where T : class
		{
			services
				.AddScoped<T>()
				.AddHttpClient<T>(
					(s, c) =>
					{
						var mcConfig = s.GetRequiredService<MultiChainConfiguration>();
						c.BaseAddress = new Uri($"http://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
					})
					.SetHandlerLifetime(HandlerLifeTime)  //Set lifetime to five minutes
					.AddPolicyHandler(ExceptionPolicyHandler.RetryPolicy())
					.AddPolicyHandler(ExceptionPolicyHandler.TimeoutPolicy())
					.ConfigurePrimaryHttpMessageHandler((s) =>
					{
						var mcConfig = s.GetRequiredService<MultiChainConfiguration>();
						return new HttpClientHandler()
						{
							Credentials = new NetworkCredential(mcConfig.Node.RpcUserName, mcConfig.Node.RpcPassword)
						};
					});
			return services;
		}

		// Override any existing MultiChainConfiguration registered in container
		public static IServiceCollection AddMultiChain(this IServiceCollection services, MultiChainConfiguration mcConfig)
		{
			var hasConfig = services.Any(x => x.ServiceType == typeof(MultiChainConfiguration));
			if (!hasConfig)
				services.AddSingleton(mcConfig);
			else
				services.Replace(new ServiceDescriptor(typeof(MultiChainConfiguration), mcConfig));
			services.AddMultiChain();
			return services;
		}

		public static IServiceCollection AddMultiChain(this IServiceCollection services)
		{
			services
				.ConfigureHttpClient<MultiChainAddressCommand>()
				.ConfigureHttpClient<MultiChainTransactionCommand>()
				.ConfigureHttpClient<MultiChainAssetCommand>()
				.ConfigureHttpClient<MultiChainStreamCommand>()
				.ConfigureHttpClient<MultiChainTokenCommand>()
				.ConfigureHttpClient<MultiChainBinaryCommand>()
				.ConfigureHttpClient<MultiChainPermissionCommand>()
				.ConfigureHttpClient<MultiChainBlockchainCommand>()
				.ConfigureHttpClient<MultiChainVariableCommand>()
				.ConfigureHttpClient<MultiChainFilterCommand>()
				;
			return services;
		}

		// multichain
		//	  - MultiChainConfiguration: must contain 1 active MultiChainNode
		//	  - MultiChainNodes (Optional): List of MultiChainNodes. Mainly used for simulation and testing purpose.
		public static IServiceCollection AddMultiChain(this IServiceCollection services, IConfiguration config)
		{
			var mcConfig = config.GetSection("MultiChainConfiguration").Get<MultiChainConfiguration>();
			if (mcConfig is { })
			{
				if (mcConfig.Node is null)
					throw new System.Exception("MultiChainConfiguration.Node not configured.");
				services
					.AddMultiChain(mcConfig)
					;
				var mcNodes = config.GetSection("MultiChainNodes").Get<IList<MultiChainNode>>();
				if (mcNodes is { })
					services.AddSingleton(mcNodes);
			}
			return services;
		}
	}
}
