using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.Base
{
	public class MultiChainCommandBase
	{
		protected readonly ILogger _logger;
		protected MultiChainConfiguration _mcConfig;
		protected HttpClient _httpClient;
		protected MultiChainConfiguration MultiChainConfiguration => _mcConfig;

		public MultiChainCommandBase(ILogger logger, MultiChainConfiguration mcConfg)
		{
			_mcConfig = mcConfg;
			_logger = logger;
			var handler = new HttpClientHandler()
			{
				Credentials = new NetworkCredential(_mcConfig.Node.RpcUserName, _mcConfig.Node.RpcPassword)
			};
			_httpClient = new HttpClient(handler)
			{
				BaseAddress = new Uri($"http://{_mcConfig.Node.NetworkAddress}:{_mcConfig.Node.NetworkPort}/")
			};
		}


		public MultiChainCommandBase(ILogger logger, MultiChainConfiguration mcConfg, HttpClient httpClient)
		{
			_logger = logger;
			_mcConfig = mcConfg;
			_httpClient = httpClient;
		}

		private string ToCommand(string method,params object[] args)
		{
			StringBuilder sb = new StringBuilder(method);
			foreach (var arg in args)
			{
				Type t = arg.GetType();
				if (!(t.IsPrimitive || t.IsValueType || (t == typeof(string))))
					sb.Append($" '{JsonConvert.SerializeObject(arg)}'");
				else
					sb.Append($" {arg}");
			}
			return sb.ToString();
		}

		/// <summary>
		/// The call is a little complex because the result is untyped and can be treated differently.
		/// a) Result can be a json object containing either error or result. If the json object contains error 
		/// then the error object contains code and message. If result and error are null then result is null.
		/// b) Result can be a string.
		/// c) Result can be a bool.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="method"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		protected async Task<MultiChainResult<T>> JsonRpcRequestAsync<T>(string method, params object[] args)
		{
			Dictionary<string, object> mcArgs = new Dictionary<string, object>()
			{
				{ "method", method },
				{ "chain_name", _mcConfig.Node.ChainName },
				{ "params", args }
			};
			string content = null;
			try
			{
				string jsonRpcRequest = JsonConvert.SerializeObject(mcArgs, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
				_logger.LogDebug($"multichain command: {ToCommand(method,args)}");
				_logger.LogTrace($"multichain request: {JValue.Parse(jsonRpcRequest).ToString(Formatting.Indented)}");
				var response = await _httpClient.PostAsync("", new StringContent(jsonRpcRequest, Encoding.UTF8, "text/plain"));
				content = await response.Content.ReadAsStringAsync();
				var result = MultiChainResultParser.ParseMultiChainResult<T>(content);
				if (result.IsError)
					throw result.Exception;
				_logger.LogTrace(JsonConvert.SerializeObject(result.Result,Formatting.Indented));
				return result;
			}
			catch (Exception ex)
			{
				var exceptionMessage = $"exception: {ex.ToString()}";
				if (content is { })
					exceptionMessage = $"response: {JValue.Parse(content).ToString(Formatting.Indented)} exception: {ex.ToString()}";
				_logger.LogWarning(exceptionMessage);
				return new MultiChainResult<T>(ex);
			}
		}

	}
}
