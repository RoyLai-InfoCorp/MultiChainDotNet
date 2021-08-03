﻿using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using NLog.Extensions.Logging;
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

		private void Initialize()
		{
			var host = Host.CreateDefaultBuilder()
				.ConfigureLogging(logging => {
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
					ConfigureServices(services);
				});

			_container = host.Build().Services;
			_loggerFactory = _container.GetRequiredService<ILoggerFactory>();
		}

		public TestBase()
		{
			Initialize();
		}

	}
}
