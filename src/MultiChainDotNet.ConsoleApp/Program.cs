using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Fluent.Signers;
using MultiChainDotNet.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Extensions.Http;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

var app = new CommandLineApplication();

app.HelpOption();

var chainConfigPath = app.Option("-c|--chain-config <path>", "MultiChainDotNet chain configuration file", CommandOptionType.SingleOrNoValue);
//chainConfigPath.DefaultValue = "chain-config.json";

var nodeConfigPath = app.Option("-n|--node-config <path>", "MultiChainDotNet node configuration file", CommandOptionType.SingleOrNoValue);
//nodeConfigPath.DefaultValue = "seednode-config.json";

var managerName = app
	.Argument("ManagerName", "Transaction | Asset | Stream | Permission | Address")
	.IsRequired(false, "ManagerName is required.");
var methodName = app
	.Argument("MethodName", "Method name belonging to the manager")
	.IsRequired(false, "MethodName is required.");
var methodArgs = app.Argument("MethodArgs", "Method arguments",true);

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
	return HttpPolicyExtensions
		.HandleTransientHttpError()
		.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
		.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
																	retryAttempt)));
}

IServiceProvider _container;
ILogger _logger=null;
MultiChainConfiguration _mcConfig;

void Initialize()
{
	string chainConfig = chainConfigPath.Value();
	if (!chainConfigPath.HasValue())
	{
		chainConfig = "chain-config.json";
		if (Environment.GetEnvironmentVariable("__MC_CHAIN_CONFIG_PATH") is { })
		{
			//chainConfig = Environment.GetEnvironmentVariable("__MC_CHAIN_CONFIG_PATH");
			chainConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.GetEnvironmentVariable("__MC_CHAIN_CONFIG_PATH"));
		}
	}
	if (!File.Exists(chainConfig))
	{
		Console.WriteLine($"{chainConfig} not found.");
		Environment.Exit(0);
	}

	string nodeConfig = nodeConfigPath.Value();
	if (!nodeConfigPath.HasValue())
	{
		nodeConfig = "seednode-config.json";
		if (Environment.GetEnvironmentVariable("__MC_NODE_CONFIG_PATH") is { })
		{
			nodeConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.GetEnvironmentVariable("__MC_NODE_CONFIG_PATH"));
			//nodeConfig = Environment.GetEnvironmentVariable("__MC_NODE_CONFIG_PATH");
		}
	}
	if (!File.Exists(nodeConfig))
	{
		Console.WriteLine($"{nodeConfig} not found.");
		Environment.Exit(0);
	}


	var host = Host.CreateDefaultBuilder()
	.ConfigureLogging(logging => {
		logging.ClearProviders();
		logging.AddConsole();
		logging.AddFilter((provider, category, logLevel) =>
		{
			if (logLevel >= LogLevel.Debug && category.Contains("MultiChainDotNet"))
				return true;
			return false;
		});
	})
	.ConfigureAppConfiguration((hostingContext, config) =>
	{
		config.AddJsonFile(chainConfig);
		config.AddJsonFile(nodeConfig);
	})
	.ConfigureServices((hostContext, services) =>
	{
		var mcConfig = hostContext.Configuration.GetSection("MultiChainConfiguration").Get<MultiChainConfiguration>();
		mcConfig.Node = hostContext.Configuration.GetSection("MultiChainNode").Get<MultiChainNode>();
		services
			.AddSingleton(mcConfig)
			.AddTransient<IMultiChainAddressManager, MultiChainAddressManager>()
			.AddTransient<IMultiChainAssetManager, MultiChainAssetManager>()
			.AddTransient<MultiChainAssetManager, MultiChainAssetManager>()
			.AddTransient<IMultiChainPermissionsManager, MultiChainPermissionsManager>()
			.AddTransient<IMultiChainStreamManager, MultiChainStreamManager>()
			.AddTransient<IMultiChainTransactionManager, MultiChainTransactionManager>()
			.AddHttpClient<IMultiChainCommandFactory, MultiChainCommandFactory>(c => c.BaseAddress = new Uri($"http://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}"))
			.SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
			.AddPolicyHandler(GetRetryPolicy())
			.ConfigurePrimaryHttpMessageHandler(() =>
			{
				return new HttpClientHandler()
				{
					Credentials = new NetworkCredential(mcConfig.Node.RpcUserName, mcConfig.Node.RpcPassword)
				};
			})
			;

	});
	_container = host.Build().Services;
	_logger = _container.GetRequiredService<ILoggerFactory>().CreateLogger("MultiChainDotNet.ConsoleApp");
	_mcConfig = _container.GetRequiredService<MultiChainConfiguration>();
}

async Task GetAssetInfoAsync()
{
	var assetMgr = _container.GetRequiredService<IMultiChainAssetManager>();
	var assetName = methodArgs.Values[0];
	var result = await assetMgr.GetAssetInfoAsync(assetName);
	Console.WriteLine(JsonConvert.SerializeObject(result.Result, Formatting.Indented));
}

async Task SendAssetAsync()
{
	var assetMgr = _container.GetRequiredService<IMultiChainAssetManager>();
	var signer = new DefaultSigner(_mcConfig.Node.Ptekey);
	string from = _mcConfig.Node.NodeWallet;
	var to = methodArgs.Values[0];
	var assetName = methodArgs.Values[1];
	var amt = UInt64.Parse(methodArgs.Values[2]);
	var data = methodArgs.Values.Count > 3 ? JToken.Parse(methodArgs.Values[3]) : null;
	var sendAssetResult = await assetMgr.SendAssetAsync(signer, from, to, assetName, amt, data);
	if (sendAssetResult.IsError)
	{
		Console.WriteLine(sendAssetResult.ExceptionMessage);
		return;
	}
	Console.WriteLine(JsonConvert.SerializeObject(sendAssetResult.Result, Formatting.Indented));

}

async Task SendAnnotateAssetAsync()
{
	var assetMgr = _container.GetRequiredService<IMultiChainAssetManager>();
	var signer = new DefaultSigner(_mcConfig.Node.Ptekey);
	string from = _mcConfig.Node.NodeWallet;
	var to = methodArgs.Values[0];
	var assetName = methodArgs.Values[1];
	var amt = UInt64.Parse(methodArgs.Values[2]);
	var data = methodArgs.Values.Count > 3 ? JToken.Parse(methodArgs.Values[3]) : null;
	var sendAssetResult = await assetMgr.SendAnnotateAssetAsync(signer, from, to, assetName, amt, data);
	if (sendAssetResult.IsError)
	{
		Console.WriteLine(sendAssetResult.ExceptionMessage);
		return;
	}
	Console.WriteLine(JsonConvert.SerializeObject(sendAssetResult.Result, Formatting.Indented));

}

async Task IssueAsync()
{
	var assetMgr = _container.GetRequiredService<IMultiChainAssetManager>();
	var signer = new DefaultSigner(_mcConfig.Node.Ptekey);
	string from = _mcConfig.Node.NodeWallet;
	var to = methodArgs.Values[0];
	var assetName = methodArgs.Values[1];
	var amt = UInt64.Parse(methodArgs.Values[2]);
	var canIssueMore = bool.Parse(methodArgs.Values[3]);
	var data = methodArgs.Values.Count > 4 ? JToken.Parse(methodArgs.Values[4]) : null;
	var result = await assetMgr.IssueAsync(signer, from, to, assetName, amt, canIssueMore, data);
	if (result.IsError)
	{
		Console.WriteLine(result.ExceptionMessage);
		return;
	}
	Console.WriteLine(JsonConvert.SerializeObject(result.Result, Formatting.Indented));
}

async Task IssueMoreAsync()
{
	var assetMgr = _container.GetRequiredService<IMultiChainAssetManager>();
	var signer = new DefaultSigner(_mcConfig.Node.Ptekey);
	string from = _mcConfig.Node.NodeWallet;
	var to = methodArgs.Values[0];
	var assetName = methodArgs.Values[1];
	var amt = UInt64.Parse(methodArgs.Values[2]);
	var data = methodArgs.Values.Count > 3 ? JToken.Parse(methodArgs.Values[3]) : null;
	var result = await assetMgr.IssueMoreAsync(signer, from, to, assetName, amt, data);
	if (result.IsError)
	{
		Console.WriteLine(result.ExceptionMessage);
		return;
	}
	Console.WriteLine(JsonConvert.SerializeObject(result.Result, Formatting.Indented));
}


async Task ListAssetBalancesByAddressAsync()
{
	var assetMgr = _container.GetRequiredService<IMultiChainAssetManager>();
	var address = methodArgs.Values[0];
	var result = await assetMgr.ListAssetBalancesByAddressAsync(address);
	if (result.IsError)
	{
		Console.WriteLine(result.ExceptionMessage);
		return;
	}
	Console.WriteLine(JsonConvert.SerializeObject(result.Result, Formatting.Indented));
}

async Task PayAsync()
{
	var assetMgr = _container.GetRequiredService<IMultiChainAssetManager>();
	var signer = new DefaultSigner(_mcConfig.Node.Ptekey);
	string from = _mcConfig.Node.NodeWallet;
	var to = methodArgs.Values[0];
	var amt = UInt64.Parse(methodArgs.Values[1]);
	var data = methodArgs.Values.Count > 2 ? JToken.Parse(methodArgs.Values[2]) : null;
	var sendAssetResult = await assetMgr.PayAsync(signer, from, to, amt, data);
	if (sendAssetResult.IsError)
	{
		Console.WriteLine(sendAssetResult.ExceptionMessage);
		return;
	}
	Console.WriteLine(JsonConvert.SerializeObject(sendAssetResult.Result, Formatting.Indented));
}


async Task GrantAsync()
{
	var permMgr = _container.GetRequiredService<IMultiChainPermissionsManager>();
	var signer = new DefaultSigner(_mcConfig.Node.Ptekey);
	string from = _mcConfig.Node.NodeWallet;
	var to = methodArgs.Values[0];
	string permission = methodArgs.Values[1];
	string entity = methodArgs.Values[2];
	var result = await permMgr.GrantPermissionAsync(signer, from, to, permission, entity);
	if (result.IsError)
	{
		Console.WriteLine(result.ExceptionMessage);
		return;
	}
	Console.WriteLine(JsonConvert.SerializeObject(result.Result, Formatting.Indented));
}

async Task CreateStreamAsync(string[] args)
{
	var streamMgr = _container.GetRequiredService<IMultiChainStreamManager>();
	var signer = new DefaultSigner(_mcConfig.Node.Ptekey);
	string from = _mcConfig.Node.NodeWallet;
	string streamName = methodArgs.Values[0];
	bool anyoneCanWrite = bool.Parse(methodArgs.Values[1]);
	var result = await streamMgr.CreateStreamAsync(signer, from, streamName, anyoneCanWrite);
	if (result.IsError)
	{
		Console.WriteLine(result.ExceptionMessage);
		return;
	}
	Console.WriteLine(JsonConvert.SerializeObject(result.Result, Formatting.Indented));
}

async Task ListStreamsAsync()
{
	var streamMgr = _container.GetRequiredService<IMultiChainStreamManager>();
	bool verbose = methodArgs.Values.Count > 1 ? bool.Parse(methodArgs.Values[1]) : false;
	var result = await streamMgr.ListStreamsAsync(verbose);
	if (result.IsError)
	{
		Console.WriteLine(result.ExceptionMessage);
		return;
	}
	Console.WriteLine(JsonConvert.SerializeObject(result.Result, Formatting.Indented));
}


async Task Execute()
{
	switch (managerName.Value)
	{
		case "Asset":
			switch (methodName.Value)
			{
				case "GetAssetInfoAsync":
					await GetAssetInfoAsync();
					return;
				case "SendAssetAsync":
					await SendAssetAsync();
					return;
				case "SendAnnotateAssetAsync":
					await SendAnnotateAssetAsync();
					return;
				case "IssueAsync":
					await IssueAsync();
					return;
				case "IssueMoreAsync":
					await IssueMoreAsync();
					return;
				case "ListAssetBalancesByAddressAsync":
					await ListAssetBalancesByAddressAsync();
					return;
				case "PayAsync":
					await PayAsync();
					return;
			}
			break;
		case "Permission":
			switch(methodName.Value)
			{
				case "GrantAsync":
					await GrantAsync();
					return;
			}
			break;
		case "Stream":
			switch(methodName.Value)
			{
				case "CreateStreamAsync":
					await CreateStreamAsync(args);
					return;
				case "ListStreamsAsync":
					await ListStreamsAsync();
					return;

			}
			break;
	}
	Console.WriteLine("Command not found.");
}

app.OnExecuteAsync(async cancellationToken =>
{
	try
	{
		Initialize();
		await Execute();
	}
	catch(Exception ex)
	{
		Console.WriteLine(ex.ToString());
	}
	Environment.Exit(0);
});

return await app.ExecuteAsync(args);