using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent.Base;
using MultiChainDotNet.Fluent.Signers;
using System.Collections.Generic;

namespace MultiChainDotNet.Fluent.Builders
{
	public interface INormalTransactionBuilder : ITransactionBuilder
	{
		#region Normal Transaction
		INormalTransactionBuilder AddSigner(SignerBase signer);
		INormalTransactionBuilder Sign();
		string RawSigned();
		string Send();

		#endregion

	}
}