// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests
{
    public class TestCommandFactoryBase: TestBase
    {
		static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
		{
			return HttpPolicyExtensions
				.HandleTransientHttpError()
				.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
																			retryAttempt)));
		}

		protected override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);
			services
				.AddHttpClient<IMultiChainCommandFactory, MultiChainCommandFactory>(c => c.BaseAddress = new Uri($"http://{_mcConfig.Node.NetworkAddress}:{_mcConfig.Node.NetworkPort}"))
					.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
					.AddPolicyHandler(GetRetryPolicy())
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
