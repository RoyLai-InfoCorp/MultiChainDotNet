using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiChainDotNet.Api.Service.Controllers
{
	public class JsonRpcCommand : MultiChainCommandBase
	{
		public JsonRpcCommand(ILogger<JsonRpcCommand> logger, MultiChainConfiguration mcConfig, HttpClient httpClient) : base(logger, mcConfig, httpClient)
		{
		}

		public Task<MultiChainResult<JToken>> JsonRpcRequestAsync(string method, params object[] args)
		{
			return base.JsonRpcRequestAsync<JToken>(method, args);
		}

	}
}
