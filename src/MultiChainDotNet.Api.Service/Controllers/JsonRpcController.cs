using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Api.Abstractions;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

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
		ILogger _logger;

		public JsonRpcController(ILogger<JsonRpcController> logger, JsonRpcCommand rpc)
		{
			_logger = logger;
			_rpc = rpc;
		}

		[HttpPost()]
		public async Task<ActionResult<JToken>> Execute(JsonRpcRequest request)
		{
			_logger.LogInformation($"JsonRpcRequest - {request.ToJson()}");
			MultiChainResult<JToken> result;
			if (request.Params.Length == 1 && request.Params[0] is null)
				result = await _rpc.JsonRpcRequestAsync(request.Method);
			else
				result = await _rpc.JsonRpcRequestAsync(request.Method, request.Params);
			if (result.IsError)
				return BadRequest(result.ExceptionMessage);
			return Ok(result.Result);
		}
	}
}
