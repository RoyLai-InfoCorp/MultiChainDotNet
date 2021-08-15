using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainTransaction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainTransactionManager
	{
		Task<MultiChainResult<List<ListAddressTransactionResult>>> ListTransactionsByAddress(string address, int count=10, int skip=0, bool verbose=false);

		Task<MultiChainResult<string>> GetAnnotationAsync(string assetName, string txid);

		Task<MultiChainResult<string>> GetDeclarationAsync(string txid);
	}
}