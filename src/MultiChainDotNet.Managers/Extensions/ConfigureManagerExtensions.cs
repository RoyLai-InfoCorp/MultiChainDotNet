// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using MultiChainDotNet.Core;

namespace MultiChainDotNet.Managers.Extensions
{
	public static class ConfigureManagerExtensions
	{
		public static IServiceCollection AddMultiChainManagers(this IServiceCollection services)
		{
			return services
				.AddScoped<IMultiChainStreamManager, MultiChainStreamManager>()
				.AddScoped<IMultiChainPermissionsManager, MultiChainPermissionsManager>()
				.AddScoped<IMultiChainAssetManager, MultiChainAssetManager>()
				.AddScoped<IMultiChainTokenManager, MultiChainTokenManager>()
				.AddScoped<IMultiChainTransactionManager, MultiChainTransactionManager>()
				.AddScoped<IMultiChainAddressManager, MultiChainAddressManager>()
				.AddScoped<IMultiChainBlockchainManager, MultiChainBlockchainManager>()
				.AddScoped<IMultiChainMultiSigManager, MultiChainMultiSigManager>()
				.AddScoped<IMultiChainVariableManager, MultiChainVariableManager>()
				;
		}
	}
}
