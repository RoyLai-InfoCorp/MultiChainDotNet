// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using System;
using System.Net;
using System.Net.Http;

namespace MultiChainDotNet.Tests.IntegrationTests
{
	public class TestCommandFactoryBase : TestBase
	{

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services
				.AddHttpClient<IMultiChainCommandFactory, MultiChainCommandFactory>(c => c.BaseAddress = new Uri($"http://{_mcConfig.Node.NetworkAddress}:{_mcConfig.Node.NetworkPort}"))
					.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
					.AddPolicyHandler(ExceptionPolicyHandler.RetryPolicy())
					.ConfigurePrimaryHttpMessageHandler(() =>
					{
						return new HttpClientHandler()
						{
							Credentials = new NetworkCredential(_mcConfig.Node.RpcUserName, _mcConfig.Node.RpcPassword)
						};
					})
				;
		}

		protected override void ConfigureLogging(ILoggingBuilder logging)
		{
			base.ConfigureLogging(logging);
			logging.AddFilter((provider, category, logLevel) =>
			{
				if (logLevel < Microsoft.Extensions.Logging.LogLevel.Warning && category.StartsWith("System.Net.Http.HttpClient"))
					return false;
				return true;
			});

		}
	}
}
