// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Configuration;
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

		public static IServiceCollection AddMultiChain(this IServiceCollection services, MultiChainConfiguration mcConfig)
		{
			var hasConfig = services.Any(x => x.ServiceType == typeof(MultiChainConfiguration));
			if (!hasConfig)
				services.AddSingleton(mcConfig);
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
				;
			return services;
		}

	}
}
