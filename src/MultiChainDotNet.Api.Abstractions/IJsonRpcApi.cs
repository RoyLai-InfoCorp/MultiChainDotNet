using Newtonsoft.Json.Linq;
using Refit;
using System.Threading.Tasks;

namespace MultiChainDotNet.Api.Abstractions
{
	public interface IJsonRpcApi
	{
		[Post("/JsonRpc")]
		public Task<JToken> Execute(JsonRpcRequest request);
	}
}
