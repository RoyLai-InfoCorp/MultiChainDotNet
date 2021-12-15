// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using System;
using System.Net.Http;

namespace MultiChainDotNet.Core
{
	public class MultiChainCommandFactory : IMultiChainCommandFactory
	{
		IServiceProvider _container;
		private readonly HttpClient _httpClient;
		MultiChainConfiguration _mcConfig;

		public MultiChainCommandFactory(IServiceProvider container, MultiChainConfiguration mcConfig, HttpClient httpClient)
		{
			_container = container;
			_httpClient = httpClient;
			_mcConfig = mcConfig;
		}

		/// <summary>
		/// Bypass DI
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T CreateCommand<T>() where T : MultiChainCommandBase
		{
			var logger = _container.GetRequiredService<ILogger<T>>();
			return (T)Activator.CreateInstance(typeof(T), logger, _mcConfig, _httpClient);
		}
	}
}
