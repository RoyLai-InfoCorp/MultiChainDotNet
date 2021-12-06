using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MultiChainDotNet.Api.Service.Controllers
{
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
		public async Task<JToken> Execute(JsonRpcRequest request)
		{
			var result = await _rpc.JsonRpcRequestAsync(request.Method,request.Params);
			if (result.IsError)
				return result.ExceptionMessage;
			return result.Result;
		}

	}
}
