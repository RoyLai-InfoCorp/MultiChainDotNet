using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainBlockchain;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainBlockchainManager
	{
		Task<MultiChainResult<GetBlockResult>> GetCurrentBlock();
	}
}