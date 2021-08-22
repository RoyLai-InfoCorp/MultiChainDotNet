using MultiChainDotNet.Core.Base;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core
{
    public static class ExceptionPolicyHandler
    {
		public static IAsyncPolicy<HttpResponseMessage> FilterMultiChainErrorPolicy = 
			Policy
				.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.InternalServerError)
				.FallbackAsync( async (result, ctx, cancel) =>
					{
						var content = await result.Result.Content.ReadAsStringAsync();
						if (result.Result.StatusCode == HttpStatusCode.InternalServerError)
						{
							var ex = MultiChainResultParser.ParseError(content);

							// Only retry these exceptions
							switch (ex.Code)
							{
								case MultiChainErrorCode.RPC_TRANSACTION_REJECTED:
								case MultiChainErrorCode.RPC_TRANSACTION_ERROR:
								case MultiChainErrorCode.RPC_CLIENT_IN_INITIAL_DOWNLOAD:
								case MultiChainErrorCode.RPC_IN_WARMUP:
									return result.Result;
								default:
									break;
							}

							// Throw MultiChain Exception to escape retry
							throw ex;
						}
						return result.Result;
					}
					, (r,c)=> Task.CompletedTask )
				;


		public static IAsyncPolicy<HttpResponseMessage> RetryPolicy(int retries = 3) =>
			HttpPolicyExtensions
				.HandleTransientHttpError()
				.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
				.Or<TimeoutRejectedException>()
				.WaitAndRetryAsync(retries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)))
			.WrapAsync(FilterMultiChainErrorPolicy)
			;

		public static IAsyncPolicy<HttpResponseMessage> TimeoutPolicy(int seconds = 2) =>
				Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(seconds));
	}
}
