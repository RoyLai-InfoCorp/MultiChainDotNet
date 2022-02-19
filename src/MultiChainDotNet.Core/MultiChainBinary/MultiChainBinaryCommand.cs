using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UtilsDotNet;

namespace MultiChainDotNet.Core.MultiChainBinary
{
    public class MultiChainBinaryCommand : MultiChainCommandBase
	{
		public MultiChainBinaryCommand(ILogger<MultiChainBinaryCommand> logger, MultiChainConfiguration mcConfig) : base(logger, mcConfig)
		{
			_logger.LogDebug($"Initialized MultiChainBinaryCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}
		public MultiChainBinaryCommand(ILogger<MultiChainBinaryCommand> logger, MultiChainConfiguration mcConfig, HttpClient httpClient) : base(logger, mcConfig, httpClient)
		{
			_logger.LogDebug($"Initialized MultiChainBinaryCommand: {mcConfig.Node.Protocol}://{mcConfig.Node.NetworkAddress}:{mcConfig.Node.NetworkPort}");
		}

		/// <summary>
		/// createbinarycache
		/// </summary>
		public Task<MultiChainResult<string>> CreateBinaryCacheAsync()
		{
			return JsonRpcRequestAsync<string>("createbinarycache");
		}

		/// <summary>
		/// appendbinarycache identifier data-hex
		/// </summary>
		public Task<MultiChainResult<Int64>> AppendBinaryCacheAsync(string id, string hex="")
		{
			if (id is null)
				throw new ArgumentNullException(nameof(id));

			return JsonRpcRequestAsync<Int64>("appendbinarycache",id,hex);
		}

		/// <summary>
		/// deletebinarycache identifier
		/// </summary>
		public Task<MultiChainResult<VoidType>> DeleteBinaryCacheAsync(string id)
		{
			if (id is null)
				throw new ArgumentNullException(nameof(id));

			return JsonRpcRequestAsync<VoidType>("deletebinarycache", id);
		}

		/// <summary>
		/// txouttobinarycache identifier txid vout (count-bytes=INT_MAX) (start-byte=0)
		/// </summary>
		public Task<MultiChainResult<Int64>> TxoutToBinaryCacheAsync(string id, string txid, int vout, int bytesCount, int bytesStart=0)
		{
			if (id is null)
				throw new ArgumentNullException(nameof(id));

			return JsonRpcRequestAsync<Int64>("txouttobinarycache", id, txid, vout, bytesCount, bytesStart);

		}

	}
}
