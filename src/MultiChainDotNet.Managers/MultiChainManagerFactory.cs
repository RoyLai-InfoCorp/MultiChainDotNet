using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
