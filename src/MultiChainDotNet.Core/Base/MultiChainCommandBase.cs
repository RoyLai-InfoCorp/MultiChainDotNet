// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UtilsDotNet.Extensions;

namespace MultiChainDotNet.Core.Base
{
	public class MultiChainCommandBase
	{
		protected readonly ILogger _logger;
		protected MultiChainConfiguration _mcConfig;
		protected HttpClient _httpClient;
		protected MultiChainConfiguration MultiChainConfiguration => _mcConfig;
		public MultiChainCommandBase(ILogger logger, MultiChainConfiguration mcConfig)
		{
			_mcConfig = mcConfig;
			_logger = logger;
			if (_mcConfig.Node is null)
				throw new MultiChainException(MultiChainErrorCode.CONFIG_NODE_MISSING);

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

		private string ToCommand(string method, params object[] args)
		{
			StringBuilder sb = new StringBuilder(method);

			if (args is null)
				return sb.ToString();

			foreach (var arg in args)
			{
				if (arg is null)
					throw new Exception("MultiChain command arguments cannot contain null");
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
			string jsonRpcRequest = JsonConvert.SerializeObject(mcArgs, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			string jsonRpcRequestFormatted = mcArgs.ToJson();
			try
			{
				var cmd = ToCommand(method, args);
				_logger.LogDebug("MultiChainRequest: POST {MultiChainMethod} {MultiChainArgs}", method, jsonRpcRequestFormatted);
				var response = await _httpClient.PostAsync("", new StringContent(jsonRpcRequest, Encoding.UTF8, "text/plain"));

				// handle non status-500 errors
				if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.InternalServerError)
					throw new Exception(response.ReasonPhrase);

				// Parse content to get result or error
				content = await response.Content.ReadAsStringAsync();
				var result = MultiChainResultParser.ParseMultiChainResult<T>(content);

				// handle status-500 but is non-multichain errors
				if (!response.IsSuccessStatusCode && !result.IsError)
					throw new Exception(response.ReasonPhrase);

				// handle multichain errors
				if (result.IsError)
					throw result.Exception;

				_logger.LogDebug("MultiChainResponse: {MultiChainMethod} - {MultiChainResponse}", method, JsonConvert.SerializeObject(result.Result, Formatting.Indented));
				return result;
			}
			catch (TimeoutRejectedException trex)
			{
				var exceptionMessage = $"MultiChainError: Polly timeout. Call: {method} - {jsonRpcRequestFormatted}";
				_logger.LogWarning(exceptionMessage);
				return new MultiChainResult<T>(new Exception(exceptionMessage, trex));
			}
			catch (Exception ex)
			{
				var exceptionMessage = $"MultiChainError: {ex.Message}. Call: {method} - {jsonRpcRequestFormatted}";
				if (content is { })
					exceptionMessage = $"{exceptionMessage} {content.ToJson()}";
				var mcException = new MultiChainResult<T>(ex);
				var shouldLogError = _mcConfig.ExcludeLogError?.Contains(((int)mcException.ErrorCode).ToString()) ?? false ? false : true;
				if (shouldLogError)
					_logger.LogWarning(exceptionMessage);
				return mcException;
			}
		}


		/// <summary>
		/// This call is used for returning untyped response as JSON object
		/// </summary>
		/// <param name="method"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public Task<MultiChainResult<JToken>> JsonRpcRequestAsync(string method, params object[] args)
		{
			return JsonRpcRequestAsync<JToken>(method, args);
		}

	}
}
