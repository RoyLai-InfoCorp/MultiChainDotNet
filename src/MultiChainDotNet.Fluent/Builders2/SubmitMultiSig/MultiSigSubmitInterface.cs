using MultiChainDotNet.Core.MultiChainTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders2.SubmitMultiSig
{
	public interface IUseMultiSigSubmit
	{
		IAddMultiSigRawTransaction IUseMultiSigSubmit();
	}

	public interface IAddMultiSigRawTransaction
	{
		IAddTransactionCommand AddMultiSigRawTransaction(string multisigRawTransaction);
	}

	public interface IAddTransactionCommand
	{
		IAddSignatures AddTransactionCommand(MultiChainTransactionCommand txnCmd);
	}


	public interface IAddSignatures
	{
		ICreateTransaction AddSignatures(List<string[]> signatures);
	}

	public interface ICreateTransaction
	{
		ISend CreateTransaction(string redeemScript);
	}

	public interface ISend
	{
		string Send();
	}


}
