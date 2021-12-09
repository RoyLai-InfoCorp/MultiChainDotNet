using Microsoft.AspNetCore.Mvc;
using MultiChainDotNet.Api.Abstractions;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;


namespace MultiChainDotNet.Api.Service.Controllers
{

	/// <summary>
	/// This controller is for proxying incoming JSON-RPC requests to the multichain node
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	public class JsonRpcController : ControllerBase
	{
		private JsonRpcCommand _rpc;

		public JsonRpcController(JsonRpcCommand rpc)
		{
			_rpc = rpc;
		}

		[HttpPost()]
		public async Task<ActionResult<JToken>> Execute(JsonRpcRequest request)
		{
			var result = await _rpc.JsonRpcRequestAsync(request.Method, request.Params);
			if (result.IsError)
				return BadRequest(result.ExceptionMessage);
			return Ok(result.Result);
		}
	}
}
