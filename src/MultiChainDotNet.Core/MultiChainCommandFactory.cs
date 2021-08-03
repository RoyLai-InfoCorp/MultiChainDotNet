using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.MultiChainTransaction;
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
	public class MultiChainCommandFactory : IMultiChainCommandFactory
	{
		IServiceProvider _container;
		HttpClient _httpClient;
		MultiChainConfiguration _mcConfig;
		public MultiChainCommandFactory(IServiceProvider provider, MultiChainConfiguration mcConfig, HttpClient httpClient)
		{
			_container = provider;
			_httpClient = httpClient;
			_mcConfig = mcConfig;
		}

		public MultiChainAddressCommand CreateMultiChainAddressCommand()
		{
			var logger = _container.GetRequiredService<ILogger<MultiChainAddressCommand>>();
			return new MultiChainAddressCommand(logger, _mcConfig, _httpClient);
		}

		public MultiChainTransactionCommand CreateMultiChainTransactionCommand()
		{
			var logger = _container.GetRequiredService<ILogger<MultiChainTransactionCommand>>();
			return new MultiChainTransactionCommand(logger, _mcConfig, _httpClient);
		}

		public MultiChainAssetCommand CreateMultiChainAssetCommand()
		{
			var logger = _container.GetRequiredService<ILogger<MultiChainAssetCommand>>();
			return new MultiChainAssetCommand(logger, _mcConfig, _httpClient);
		}

		public MultiChainStreamCommand CreateMultiChainStreamCommand()
		{
			var logger = _container.GetRequiredService<ILogger<MultiChainStreamCommand>>();
			return new MultiChainStreamCommand(logger, _mcConfig, _httpClient);
		}

		public MultiChainPermissionCommand CreateMultiChainPermissionCommand()
		{
			var logger = _container.GetRequiredService<ILogger<MultiChainPermissionCommand>>();
			return new MultiChainPermissionCommand(logger, _mcConfig, _httpClient);
		}

	}
}
