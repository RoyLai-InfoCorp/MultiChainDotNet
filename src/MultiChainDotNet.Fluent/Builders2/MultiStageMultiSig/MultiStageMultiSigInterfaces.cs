using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders2.MultiStageMultiSig
{
	public interface IUseMultiStageMultiSig
	{
		IAddMultiSigRawTransaction UseMultiStageMultiSig();
	}

	public interface IAddMultiSigRawTransaction
	{
		IAddTransactionCommand AddMultiSigRawTransaction(string multisigRawTransaction);
	}

	public interface IAddTransactionCommand
	{
		IAddSigner AddTransactionCommand(MultiChainTransactionCommand txnCmd);
	}

	public interface IAddSigner
	{
		IMultiSignPartial AddSigner(SignerBase signer);
	}

	public interface IMultiSignPartial
	{
		string[] MultiSignPartial(string redeemScript);
	}
}
