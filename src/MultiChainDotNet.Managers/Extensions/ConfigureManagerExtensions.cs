using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers.Extensions
{
    public static class ConfigureManagerExtensions
    {
        public static IServiceCollection AddMultiChainManagers(this IServiceCollection services)
		{
			return services
				.AddTransient<IMultiChainStreamManager, MultiChainStreamManager>()
				.AddTransient<IMultiChainPermissionsManager, MultiChainPermissionsManager>()
				.AddTransient<IMultiChainAssetManager, MultiChainAssetManager>()
				.AddTransient<IMultiChainTransactionManager, MultiChainTransactionManager>()
				.AddTransient<IMultiChainAddressManager, MultiChainAddressManager>()
				.AddTransient<IMultiChainBlockchainManager, MultiChainBlockchainManager>();
		}
	}
}
