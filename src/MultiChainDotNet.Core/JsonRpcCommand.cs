using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core
{

	/// <summary>
	/// Untyped command for JSON RPC API
	/// </summary>
	public class JsonRpcCommand : MultiChainCommandBase
	{
		public JsonRpcCommand(ILogger<JsonRpcCommand> logger, MultiChainConfiguration mcConfig, HttpClient httpClient) : base(logger, mcConfig, httpClient)
		{
		}
	}
}
