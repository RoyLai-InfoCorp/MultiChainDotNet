// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent;
using MultiChainDotNet.Fluent.Signers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UtilsDotNet;

namespace MultiChainDotNet.Managers
{
	public class MultiChainStreamManager : IMultiChainStreamManager
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _logger;
		private MultiChainConfiguration _mcConfig;
		protected SignerBase _defaultSigner;
		private MultiChainStreamCommand _streamCmd;
		MultiChainTransactionCommand _txnCmd;

		public MultiChainStreamManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory cmdFactory,
			MultiChainConfiguration mcConfig)
		{
			_loggerFactory = loggerFactory;
			_mcConfig = mcConfig;
			_logger = loggerFactory.CreateLogger<MultiChainStreamManager>();
			_streamCmd = cmdFactory.CreateCommand<MultiChainStreamCommand>();
			_txnCmd = cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_defaultSigner = new DefaultSigner(_mcConfig.Node.Ptekey);
		}

		public MultiChainStreamManager(ILoggerFactory loggerFactory,
			IMultiChainCommandFactory cmdFactory,
			MultiChainConfiguration mcConfig,
			SignerBase signer)
		{
			_loggerFactory = loggerFactory;
			_mcConfig = mcConfig;
			_logger = loggerFactory.CreateLogger<MultiChainStreamManager>();
			_streamCmd = cmdFactory.CreateCommand<MultiChainStreamCommand>();
			_txnCmd = cmdFactory.CreateCommand<MultiChainTransactionCommand>();
			_defaultSigner = signer;
		}

		public async Task<bool> IsExist(string streamName)
		{
			try
			{
				await SubscribeAsync(streamName);
			}
			catch (Exception e)
			{
				if (!e.IsMultiChainException(MultiChainErrorCode.RPC_ENTITY_NOT_FOUND))
				{
					_logger.LogWarning(e.ToString());
					throw;
				}
				return false;
			}
			return true;
		}

		public string CreateStream(string streamName, bool anyoneCanWrite = false)
		{
			_logger.LogDebug($"Executing CreateStreamAsync");
			return CreateStream(_defaultSigner, _mcConfig.Node.NodeWallet, streamName, anyoneCanWrite);
		}

		public string CreateStream(SignerBase signer, string fromAddress, string streamName, bool anyoneCanWrite = false)
		{
			_logger.LogDebug($"Executing CreateStream");
			try
			{
				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.With()
					.CreateStream(streamName, anyoneCanWrite)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;

				Task.Run(async () =>
				{
					await _streamCmd.SubscribeAsync(streamName);
				}).GetAwaiter().GetResult();

				return txid;
			}
			catch (Exception ex)
			{
				// Error code remapped
				if (ex.Message.Contains("New entity script rejected - entity with this name already exists."))
				{
					Exception me = new MultiChainException(MultiChainErrorCode.RPC_DUPLICATE_NAME);
					_logger.LogWarning(me.ToString());
					throw me;
				}

				_logger.LogWarning(ex.ToString());
				throw;
			}
		}

		public async Task<string> PublishJsonAsync(string streamName, string key, object json)
		{
			_logger.LogDebug($"Executing PublishJsonAsync");
			if (_defaultSigner is { })
				return await PublishJsonAsync(_defaultSigner, _mcConfig.Node.NodeWallet, streamName, key, json);

			// Remember to subscribe
			await SubscribeAsync(streamName);
			var result = await _streamCmd.PublishJsonStreamItemAsync(streamName, new string[] { key }, json);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}
			return result.Result;
		}

		public async Task<string> PublishJsonAsync(SignerBase signer, string fromAddress, string streamName, string key, object json)
		{
			_logger.LogDebug($"Executing PublishJsonAsync");
			try
			{
				// Remember to subscribe
				await SubscribeAsync(streamName);

				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.With()
					.PublishJson(streamName, key, json)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;

				return txid;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}

		public async Task<string> PublishJsonAsync(string streamName, string[] keys, object json)
		{
			_logger.LogDebug($"Executing PublishJsonAsync");

			if (_defaultSigner is { })
				return await PublishJsonAsync(_defaultSigner, _mcConfig.Node.NodeWallet, streamName, keys, json);

			var result = await _streamCmd.PublishJsonStreamItemAsync(streamName, keys, json);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}
			return result.Result;

		}

		public async Task<string> PublishJsonAsync(SignerBase signer, string fromAddress, string streamName, string[] keys, object json)
		{
			_logger.LogDebug($"Executing PublishJsonAsync");

			try
			{
				// Remember to subscribe
				await SubscribeAsync(streamName);

				var txid = new MultiChainFluent()
					.AddLogger(_logger)
					.From(fromAddress)
					.With()
					.PublishJson(streamName, keys, json)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(signer)
						.Sign()
						.Send()
					;

				return txid;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.ToString());
				throw;
			}
		}


		public async Task SubscribeAsync(string streamName)
		{
			_logger.LogDebug($"Executing SubscribeAsync");
			var result = await _streamCmd.SubscribeAsync(streamName);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}
		}

		public async Task<StreamsResult> GetStreamAsync(string streamName)
		{
			_logger.LogDebug($"Executing GetStreamAsync");

			var result = await _streamCmd.ListStreamsAsync(streamName, true);

			if (result.IsError && result.Exception.IsMultiChainException(MultiChainErrorCode.RPC_ENTITY_NOT_FOUND))
				return null;

			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}

			return result.Result[0];
		}


		/// <summary>
		/// Returns all items from streams containing Json stream item of type T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="streamName"></param>
		/// <returns></returns>
		public async Task<List<T>> ListAllStreamItemsAsync<T>(string streamName)
		{
			int page = 0;
			int size = 20;
			bool empty = false;
			var streamItems = new List<T>();
			while (!empty)
			{
				IList<StreamItemsResult> result = null;
				try
				{
					result = await ListStreamItemsAsync($"FROM {streamName} ASC PAGE {page} SIZE {size}");
					if (result.Count == 0)
						empty = true;
					else
					{
						foreach (var item in result)
						{
							try
							{
								var json = ConvertMultiChainJsonResult<T>(item);
								streamItems.Add(json);
							}
							catch (Exception ex)
							{
								if (!ex.ToString().Contains("cast"))
									throw;
							}
						}
					}
					page++;
				}
				catch (Exception ex)
				{
					if (ex.IsMultiChainException(MultiChainErrorCode.RPC_ENTITY_NOT_FOUND))
						return null;
					throw;
				}

			}
			return streamItems;
		}

		public async Task<List<StreamsResult>> ListStreamsAsync(bool verbose = false)
		{
			_logger.LogDebug($"Executing ListStreamsAsync");

			var result = await _streamCmd.ListStreamsAsync("*", verbose);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}
			return result.Result;
		}

		public async Task<List<StreamsResult>> ListStreamsAsync(string streamName, bool verbose = false)
		{
			_logger.LogDebug($"Executing ListStreamsAsync");

			var result = await _streamCmd.ListStreamsAsync(streamName, verbose);
			if (result.IsError)
			{
				_logger.LogWarning(result.Exception.ToString());
				throw result.Exception;
			}
			return result.Result;
		}

		/// <summary>
		/// 
		/// The query syntax will return the latest items in descending order by default.
		/// 
		/// Syntax:
		/// 
		/// FROM <streamName> [WHERE (txid=<txid>|key=<key>|publish=<address>) [(DESC|ASC)] ] [PAGE page SIZE size]
		/// 
		/// Example:
		/// 
		/// 1. Get last item from <streamName>
		///    > FROM <streamName>
		/// 
		/// 2. Get first item from <streamName>
		///    > FROM <streamName> ASC
		/// 
		/// 3. Get last 5 items from <streamName> in descending order, ie. if items are 1,2,3,4,5,6,7,8,9,10 will return 10,9,8,7,6
		///    > FROM <streamName> PAGE 0 SIZE 5
		///    
		/// 4. Get first 5 items from <streamName> in ascending order, ie. if items are 1,2,3,4,5,6,7,8,9,10 will return 1,2,3,4,5
		///    > FROM <streamName> ASC PAGE 0 SIZE 5
		///    
		/// 5. Get item by txid
		///    > FROM <streamName> WHERE txid='...'
		/// 
		/// 6. Get item by key
		///    > FROM <streamName> WHERE key='...'
		///    
		/// 7. Get item by publisher wallet address
		///    > FROM <streamName> WHERE publisher='...'
		/// 
		/// </summary>
		/// <param name="selectCmd"></param>
		/// <param name="verbose"></param>
		/// <returns></returns>
		public static (string StreamName, string SearchType, string Where, string Order, int page, int size) ParseSelectStreamItems(string selectCmd)
		{
			var pattern = @"^FROM\s(?'stream'[^\s]+)(\sWHERE\s(?'type'txid|publisher|key)='(?'where'.+)')?(\s(?'order'DESC|ASC))?(\sPAGE\s(?'page'\d+)\sSIZE\s(?'size'\d+))?";

			var match = Regex.Match(selectCmd, pattern);
			if (!match.Success)
				return (null, null, null, null, 0, 0);

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

			int page = 0;
			int size = 0;
			if (match.Groups["page"].Success && match.Groups["size"].Success)
			{
				page = int.Parse(match.Groups["page"].Value);
				size = int.Parse(match.Groups["size"].Value);
			}

			return (stream, searchType, where, order, page, size);
		}

		public async Task<StreamItemsResult> GetStreamItemAsync(string selectCmd)
		{
			_logger.LogDebug($"Executing GetStreamItemAsync");

			var result = await ListStreamItemsAsync(selectCmd, true);
			if (result is null || result.Count == 0)
				return null;
			return result[0];
		}

		public async Task<IList<StreamItemsResult>> ListStreamItemsAsync(string selectCmd, bool verbose = false)
		{
			_logger.LogDebug($"Executing SelectStreamItemsAsync");

			var (streamName, searchType, where, order, page, size) = ParseSelectStreamItems(selectCmd);
			IList<StreamItemsResult> list = null;

			if (streamName is null)
				throw new Exception("Invalid FOR clause");

			order = order ?? "DESC";
			int count = 1;
			int startFrom = -1;

			if (page == 0 && size == 0)
			{
				if (order == "ASC")
					startFrom = 0;
			}
			else
			{
				count = size;
				startFrom = -((page + 1) * size);
				if (order == "ASC")
					startFrom = page * size;
			}

			if (searchType is { })
			{
				if (where is null)
					throw new Exception("Invalid WHERE clause");
				switch (searchType)
				{
					case "txid":
						var txidResult = await _streamCmd.GetStreamItemByTxidAsync(streamName, where);
						if (txidResult.IsError)
						{
							_logger.LogWarning(txidResult.Exception.ToString());
							throw txidResult.Exception;
						}
						list = new List<StreamItemsResult> { txidResult.Result };
						break;
					case "publisher":
						var pubResult = await _streamCmd.ListStreamItemsByPublisherAsync(streamName, where, verbose, count, startFrom);
						if (pubResult.IsError)
						{
							_logger.LogWarning(pubResult.Exception.ToString());
							throw pubResult.Exception;
						}
						list = pubResult.Result;
						break;
					case "key":
						var keyResult = await _streamCmd.ListStreamItemsByKeyAsync(streamName, where, verbose, count, startFrom);
						if (keyResult.IsError)
						{
							_logger.LogWarning(keyResult.Exception.ToString());
							throw keyResult.Exception;
						}
						list = keyResult.Result;
						break;
				}
			}
			else
			{
				var listResult = await _streamCmd.ListStreamItemsAsync(streamName, verbose, count, startFrom);
				if (listResult.IsError)
				{
					_logger.LogWarning(listResult.Exception.ToString());
					throw listResult.Exception;
				}
				list = listResult.Result;
			}

			if (order == "DESC")
				return list.Reverse().ToArray();

			return list;
		}

		private T ConvertMultiChainJsonResult<T>(StreamItemsResult streamItem)
		{
			var data = JToken.FromObject(streamItem.Data);
			var item = data.SelectToken("json");
			return item.ToObject<T>();
		}

		public async Task<List<T>> ListStreamItemsAsync<T>(string selectCmd)
		{
			var list = new List<T>();
			var streamItems = await ListStreamItemsAsync(selectCmd, false);
			if (streamItems is null)
				return null;

			foreach (var streamItem in streamItems)
			{
				try
				{
					var json = ConvertMultiChainJsonResult<T>(streamItem);
					list.Add(json);
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex.ToString());
					throw new Exception($"Failed to convert {JsonConvert.SerializeObject(streamItem.Data)} into {typeof(T).Name}. {ex.ToString()}");
				}
			}
			return list;
		}

		public async Task<T> GetStreamItemAsync<T>(string selectCmd)
		{
			_logger.LogDebug($"Executing GetStreamItemAsync");

			var result = await ListStreamItemsAsync<T>(selectCmd);

			if (result.Count == 0)
				return default(T);

			return result[0];
		}

	}
}
