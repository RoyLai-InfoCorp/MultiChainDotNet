using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent;
using MultiChainDotNet.Fluent.Builders;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public class MultiChainStreamManager : IMultiChainStreamManager
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;
		private IMultiChainCommandFactory _cmdFactory;
		private MultiChainConfiguration _mcConfig;
		protected SignerBase _defaultSigner;
		private MultiChainStreamCommand _streamCmd;
		MultiChainTransactionCommand _txnCmd;

		public MultiChainStreamManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory commandFactory, 
			MultiChainConfiguration mcConfig)
		{
			_loggerFactory = loggerFactory;
			_cmdFactory = commandFactory;
			_mcConfig = mcConfig;
			_logger = loggerFactory.CreateLogger<MultiChainStreamManager>();

			_streamCmd = _cmdFactory.CreateCommand<MultiChainStreamCommand>();
			_txnCmd = _cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public async Task<MultiChainResult<string>> CreateStreamAsync(string streamName, bool anyoneCanWrite = false)
		{
			_logger.LogInformation($"Executing CreateStreamAsync");

			//Note: No need to subscribe since stream hasn't exist
			if (_defaultSigner is { })
				return await CreateStreamAsync(_defaultSigner, _mcConfig.Node.NodeWallet, streamName, anyoneCanWrite);
			return await _streamCmd.CreateStreamAsync(streamName, anyoneCanWrite);
		}

		public Task<MultiChainResult<string>> CreateStreamAsync(SignerBase signer, string fromAddress, string streamName, bool anyoneCanWrite = false)
		{
			_logger.LogInformation($"Executing {new StackTrace().GetFrame(0).GetMethod().Name}");

			try
			{
				//Note: No need to subscribe since stream hasn't exist
				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.With()
					.CreateStream(streamName, anyoneCanWrite)
					;
				var raw = requestor.Request(_txnCmd);
				var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txid = txnMgr
				.AddSigner(signer)
					.Sign(raw)
					.Send()
					;
				return Task.FromResult(new MultiChainResult<string>(txid));
			}
			catch (Exception ex)
			{
				// Error code remapped
				if (ex.Message.Contains("New entity script rejected - entity with this name already exists."))
				{
					Exception me = new MultiChainException(MultiChainErrorCode.RPC_DUPLICATE_NAME);
					_logger.LogWarning(me.ToString());
					return Task.FromResult(new MultiChainResult<string>(me));
				}

				_logger.LogWarning(ex.ToString());
				return Task.FromResult(new MultiChainResult<string>(ex));
			}
		}

		public async Task<MultiChainResult<string>> PublishJsonAsync(string streamName, string key, object json)
		{
			_logger.LogInformation($"Executing PublishJsonAsync");

			if (_defaultSigner is { })
				return await PublishJsonAsync (_defaultSigner, _mcConfig.Node.NodeWallet, streamName, key, json);

			// Remember to subscribe
			await SubscribeAsync(streamName);
			return await _streamCmd.PublishJsonStreamItemAsync(streamName, new string[] { key }, json);
		}

		public async Task<MultiChainResult<string>> PublishJsonAsync(SignerBase signer, string fromAddress, string streamName, string key, object json)
		{
			_logger.LogInformation($"Executing PublishJsonAsync");
			try
			{
				// Remember to subscribe
				await SubscribeAsync(streamName);

				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.With()
					.PublishJson(streamName, key, json)
					;
				var raw = requestor.Request(_txnCmd);
				var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txid = txnMgr
				.AddSigner(signer)
					.Sign(raw)
					.Send()
					;

				return new MultiChainResult<string>(txid);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
			}
		}

		public async Task<MultiChainResult<string>> PublishJsonAsync(string streamName, string[] keys, object json)
		{
			_logger.LogInformation($"Executing PublishJsonAsync");

			if (_defaultSigner is { })
				return await PublishJsonAsync (_defaultSigner, _mcConfig.Node.NodeWallet, streamName, keys, json);

			// Remember to subscribe
			await SubscribeAsync(streamName);

			return await _streamCmd.PublishJsonStreamItemAsync(streamName, keys, json);
		}

		public async Task<MultiChainResult<string>> PublishJsonAsync(SignerBase signer, string fromAddress, string streamName, string[] keys, object json)
		{
			_logger.LogInformation($"Executing PublishJsonAsync");

			try
			{
				// Remember to subscribe
				await SubscribeAsync(streamName);

				var requestor = new TransactionRequestor();
				requestor
					.From(fromAddress)
					.With()
					.PublishJson(streamName, keys, json)
					;
				var raw = requestor.Request(_txnCmd);
				var txnMgr = new TransactionSender(_loggerFactory.CreateLogger<TransactionSender>(), _txnCmd);
				var txid = txnMgr
					.AddSigner(signer)
					.Sign(raw)
					.Send()
					;
				return new MultiChainResult<string>(txid);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<string>(ex);
			}
		}


		public async Task<MultiChainResult<VoidType>> SubscribeAsync(string streamName)
		{
			_logger.LogInformation($"Executing SubscribeAsync");

			return await _streamCmd.SubscribeAsync(streamName);
		}

		public async Task<MultiChainResult<StreamsResult>> GetStreamAsync(string streamName)
		{
			_logger.LogInformation($"Executing GetStreamAsync");

			var result = await _streamCmd.ListStreamsAsync(streamName, true);
			if (result.IsError)
				return new MultiChainResult<StreamsResult>(result.Exception);

			try
			{
				var stream = result.Result.FirstOrDefault(x => x.Name == streamName);
				return new MultiChainResult<StreamsResult>(stream);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				return new MultiChainResult<StreamsResult>(ex);
			}
		}

		public async Task<MultiChainResult<List<StreamsResult>>> ListStreamsAsync(bool verbose = false)
		{
			_logger.LogInformation($"Executing ListStreamsAsync");

			return await _streamCmd.ListStreamsAsync("*", verbose);
		}

		public async Task<MultiChainResult<List<StreamsResult>>> ListStreamsAsync(string streamName, bool verbose = false)
		{
			_logger.LogInformation($"Executing ListStreamsAsync");

			return await _streamCmd.ListStreamsAsync(streamName, verbose);
		}

		public static (string StreamName, string SearchType, string Where, string Order) ParseSelectStreamItems(string selectCmd)
		{
			var pattern = @"^FROM\s(?'stream'[^\s]+)(\sWHERE\s(?'type'txid|publisher|key)='(?'where'.+)')?(\sORDER\s(?'order'DESC|ASC))?";

			var match = Regex.Match(selectCmd, pattern);
			if (!match.Success)
				return (null, null, null, null);

			string stream = null;
			if (match.Groups["stream"].Success)
				stream = match.Groups["stream"].Value;

			string searchType = null;
			if (match.Groups["type"].Success)
				searchType = match.Groups["type"].Value;

			string where = null;
			if (match.Groups["where"].Success)
				where = match.Groups["where"].Value;

			string order = null;
			if (match.Groups["order"].Success)
				order = match.Groups["order"].Value;

			return (stream, searchType, where, order);
		}

		public async Task<MultiChainResult<StreamItemsResult>> GetStreamItemAsync(string selectCmd)
		{
			_logger.LogInformation($"Executing GetStreamItemAsync");

			var result = await SelectStreamItemsAsync(selectCmd, true, 1, -1);
			if (result.IsError)
				return new MultiChainResult<StreamItemsResult>(result.Exception);

			if (result.Result.Count == 0)
				return new MultiChainResult<StreamItemsResult>();

			return new MultiChainResult<StreamItemsResult>(result.Result[0]);
		}

		/// <summary>
		/// FROM <streamName> [WHERE (txid=<txid>|key=<key>|publish=<address>) [ORDER (desc|asc)] ]
		/// </summary>
		/// <param name="selectCmd"></param>
		/// <param name="verbose"></param>
		/// <returns></returns>
		public async Task<MultiChainResult<IList<StreamItemsResult>>> SelectStreamItemsAsync(string selectCmd, bool verbose, int count, int startFrom)
		{
			_logger.LogInformation($"Executing SelectStreamItemsAsync");

			var (streamName, searchType, where, order) = ParseSelectStreamItems(selectCmd);
			IList<StreamItemsResult> list = null;

			if (streamName is null)
				throw new Exception("Invalid FOR clause");

			if (searchType is { })
			{
				if (where is null)
					throw new Exception("Invalid WHERE clause");
				switch (searchType)
				{
					case "txid":
						var txidResult = await _streamCmd.GetStreamItemByTxidAsync(streamName, where);
						if (txidResult.IsError)
							return new MultiChainResult<IList<StreamItemsResult>>(txidResult.Exception);
						list = new List<StreamItemsResult> { txidResult.Result };
						break;
					case "publisher":
						var pubResult = await _streamCmd.ListStreamItemsByPublisherAsync(streamName, where, verbose, count, startFrom);
						if (pubResult.IsError)
							return new MultiChainResult<IList<StreamItemsResult>>(pubResult.Exception);
						list = pubResult.Result;
						break;
					case "key":
						var keyResult = await _streamCmd.ListStreamItemsByKeyAsync(streamName, where, verbose, count, startFrom);
						if (keyResult.IsError)
							return new MultiChainResult<IList<StreamItemsResult>>(keyResult.Exception);
						list = keyResult.Result;
						break;
				}
			}
			else
			{
				var listResult = await _streamCmd.ListStreamItemsAsync(streamName, verbose, count, startFrom);
				if (listResult.IsError)
					return new MultiChainResult<IList<StreamItemsResult>>(listResult.Exception);
				list = listResult.Result;
			}
			if (order == null || order.ToUpper() != "ASC")
				return new MultiChainResult<IList<StreamItemsResult>>(list.Reverse().ToArray());

			return new MultiChainResult<IList<StreamItemsResult>>(list);

		}

	}
}
