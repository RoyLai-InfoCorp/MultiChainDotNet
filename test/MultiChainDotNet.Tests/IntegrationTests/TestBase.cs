// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainTransaction;
using NLog.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Tests.IntegrationTests
{
	public class TestBase
	{
		protected IConfiguration _configRoot;
		protected ILoggerFactory _loggerFactory;
		protected IServiceProvider _container;
		protected MultiChainNode _admin;
		protected MultiChainNode _testUser1;
		protected MultiChainNode _testUser2;
		protected MultiChainNode _relayer1;
		protected MultiChainNode _relayer2;
		protected MultiChainNode _relayer3;
		protected MultiChainConfiguration _mcConfig;
		protected TestStateDb _stateDb = new TestStateDb();

		protected virtual void ConfigureServices(IServiceCollection services)
		{

		}

		protected virtual void ConfigureLogging(ILoggingBuilder logging)
		{

		}

		private IServiceCollection AddNamedHttpClient(IServiceCollection services, MultiChainNode node)
		{
			services
				.AddHttpClient(node.NodeName, c => {
					c.BaseAddress = new Uri($"http://{node.NetworkAddress}:{node.NetworkPort}/");
				})
				.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
				.AddPolicyHandler(ExceptionPolicyHandler.RetryPolicy())
				.AddPolicyHandler(ExceptionPolicyHandler.TimeoutPolicy())
				.ConfigurePrimaryHttpMessageHandler(() =>
				{
					return new HttpClientHandler()
					{
						Credentials = new NetworkCredential(node.RpcUserName, node.RpcPassword)
					};
				})
				;
			return services;
		}

		protected async Task<string> ShowDecodedTransaction(string txid)
		{
			var txCmd = _container.GetService<MultiChainTransactionCommand>();
			var result = await txCmd.GetRawTransactionAsync(txid);
			var result2 = await txCmd.DecodeRawTransactionAsync(result.Result);
			return result2.Result.ToJson();
		}

		protected string RandomName() => Guid.NewGuid().ToString("N").Substring(20);

		private void Initialize()
		{
			var host = Host.CreateDefaultBuilder()
				.ConfigureLogging(logging =>
				{
					logging.AddNLog("nlog.config");
					ConfigureLogging(logging);
				})
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.AddJsonFile("appsettings.json");
				})
				.ConfigureServices((hostContext, services) =>
				{
					_mcConfig = hostContext.Configuration.GetSection("MultiChainConfiguration").Get<MultiChainConfiguration>();
					_admin = hostContext.Configuration.GetSection("Nodes:seednode").Get<MultiChainNode>();
					_testUser1 = hostContext.Configuration.GetSection("Nodes:testuser1").Get<MultiChainNode>();
					_testUser2 = hostContext.Configuration.GetSection("Nodes:testuser2").Get<MultiChainNode>();
					_relayer1 = hostContext.Configuration.GetSection("Nodes:relayer1").Get<MultiChainNode>();
					_relayer2 = hostContext.Configuration.GetSection("Nodes:relayer2").Get<MultiChainNode>();
					_relayer3 = hostContext.Configuration.GetSection("Nodes:relayer3").Get<MultiChainNode>();
					services
						.AddSingleton(_mcConfig)
						;
					AddNamedHttpClient(services, _admin);
					AddNamedHttpClient(services, _relayer1);
					AddNamedHttpClient(services, _relayer2);
					AddNamedHttpClient(services, _relayer3);
					ConfigureServices(services);
				});

			_container = host.Build().Services;
			_loggerFactory = _container.GetRequiredService<ILoggerFactory>();
		}

		protected async Task FundWallet(string address)
		{
			var assetCmd = _container.GetRequiredService<MultiChainAssetCommand>();
			await assetCmd.SendFromAsync(_admin.NodeWallet, address, 2000);
		}

		protected async Task GrantPermissionFromNode(MultiChainNode node, string newAddr, string permission)
		{
			var http = _container.GetRequiredService<IHttpClientFactory>().CreateClient(node.NodeName);
			var addr = new MultiChainAddressCommand( _loggerFactory.CreateLogger<MultiChainAddressCommand>(), _mcConfig, http);
			await addr.ImportAddressAsync(newAddr);
			var perm1 = new MultiChainPermissionCommand(_loggerFactory.CreateLogger<MultiChainPermissionCommand>(), _mcConfig, http);
			var grant = await perm1.GrantPermissionFromAsync(node.NodeWallet, newAddr, permission);
			await Task.Delay(1000);
			if (grant.IsError) throw grant.Exception;
		}

		protected async Task RevokePermissionFromNode(MultiChainNode node, string newAddr, string permission)
		{
			var http = _container.GetRequiredService<IHttpClientFactory>().CreateClient(node.NodeName);
			var addr = new MultiChainAddressCommand(_loggerFactory.CreateLogger<MultiChainAddressCommand>(), _mcConfig, http);
			await addr.ImportAddressAsync(newAddr);
			var perm1 = new MultiChainPermissionCommand(_loggerFactory.CreateLogger<MultiChainPermissionCommand>(), _mcConfig, http);
			var grant = await perm1.RevokePermissionFromAsync(node.NodeWallet, newAddr, permission);
			await Task.Delay(1000);
			if (grant.IsError) throw grant.Exception;
		}

		public TestBase()
		{
			Initialize();
		}

	}
}
