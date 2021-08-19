using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core;
using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Core.MultiChainVariable;
using MultiChainDotNet.Fluent.Builders2;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public class MultiChainVariableManager : IMultiChainVariableManager
	{
		private readonly ILogger _logger;
		private readonly IMultiChainCommandFactory _commandFactory;
		private readonly MultiChainVariableCommand _varCmd;
		MultiChainTransactionCommand _txnCmd;
		protected SignerBase _defaultSigner;
		MultiChainConfiguration _mcConfig;

		public MultiChainVariableManager(
			ILogger<MultiChainVariableManager> logger,
			IMultiChainCommandFactory commandFactory,
			MultiChainConfiguration mcConfig)
		{
			_commandFactory = commandFactory;
			_logger = logger;
			_varCmd = commandFactory.CreateCommand<MultiChainVariableCommand>();
			_txnCmd = commandFactory.CreateCommand<MultiChainTransactionCommand>();
			_mcConfig = mcConfig;
			_defaultSigner = new DefaultSigner(mcConfig.Node.Ptekey);
		}

		public MultiChainVariableManager(
			ILogger<MultiChainVariableManager> logger,
			IMultiChainCommandFactory commandFactory,
			MultiChainConfiguration mcConfig,
			SignerBase signer)
		{
			_commandFactory = commandFactory;
			_logger = logger;
			_varCmd = commandFactory.CreateCommand<MultiChainVariableCommand>();
			_txnCmd = commandFactory.CreateCommand<MultiChainTransactionCommand>();
			_mcConfig = mcConfig;
			_defaultSigner = signer;
		}

		public MultiChainResult<string> CreateVariable(string variableName)
		{
			return CreateVariable(_defaultSigner, variableName);
		}

		public MultiChainResult<string> CreateVariable(SignerBase signer, string variableName)
		{
			_logger.LogDebug($"Executing CreateVariableAsync");
			signer = signer ?? _defaultSigner;
			try
			{
				var fromAddress = _mcConfig.Node.NodeWallet;
				var txid = new MultiChainFluentApi()
					.AddLogger(_logger)
					.From(fromAddress)
					.With()
					.CreateVariable(variableName, null)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(_defaultSigner)
						.Sign()
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

		public MultiChainResult<string> SetVariableValue(string variableName, object variableValue)
		{
			return SetVariableValue(_defaultSigner, variableName, variableValue);
		}
		public MultiChainResult<string> SetVariableValue(SignerBase signer, string variableName, object variableValue)
		{
			_logger.LogDebug($"Executing SetVariableAsync");
			signer = signer ?? _defaultSigner;
			try
			{
				var fromAddress = _mcConfig.Node.NodeWallet;
				var txid = new MultiChainFluentApi()
					.AddLogger(_logger)
					.From(fromAddress)
					.With()
					.UpdateVariable(variableName, variableValue)
					.CreateNormalTransaction(_txnCmd)
						.AddSigner(_defaultSigner)
						.Sign()
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

		public async Task<MultiChainResult<T>> GetVariableValueAsync<T>(string variableName)
		{
			return await _varCmd.GetVariableValueAsync<T>(variableName);
		}

	}
}
