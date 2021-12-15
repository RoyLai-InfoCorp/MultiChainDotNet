// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Fluent.Signers;
using System;

namespace MultiChainDotNet.Managers
{
	public class MultiChainManagerFactory : IMultiChainManagerFactory
	{
		private IServiceProvider _container;
		IMultiChainCommandFactory _cmdFactory;
		private MultiChainConfiguration _mcConfig;

		public MultiChainManagerFactory(IServiceProvider container,
			IMultiChainCommandFactory cmdFactory,
			MultiChainConfiguration mcConfig)
		{
			_container = container;
			_cmdFactory = cmdFactory;
			_mcConfig = mcConfig;
		}

		public T CreateInstance<T>(SignerBase signer)
		{
			var loggerFactory = _container.GetRequiredService<ILoggerFactory>();
			return (T)Activator.CreateInstance(typeof(T), loggerFactory, _cmdFactory, _mcConfig, signer);
		}

		public T CreateInstance<T>()
		{
			var loggerFactory = _container.GetRequiredService<ILoggerFactory>();
			return (T)Activator.CreateInstance(typeof(T), loggerFactory, _cmdFactory, _mcConfig);
		}

	}
}
