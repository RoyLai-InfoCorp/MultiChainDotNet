// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainBlockchain;
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

		public T CreateCommand<T>() where T:MultiChainCommandBase
		{
			var logger = _container.GetRequiredService<ILogger<T>>();

			return (T)Activator.CreateInstance(typeof(T), logger, _mcConfig, _httpClient);
		}
	}
}
