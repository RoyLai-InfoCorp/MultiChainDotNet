using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAddress;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainAddressManager
	{
		MultiChainResult<CreateMultiSigResult> CreateMultiSig(int nRequired, string[] pubkeys);
		Task<MultiChainResult<VoidType>> ImportAddressAsync(string address);
		Task<MultiChainResult<bool>> IsExistAsync(string address);
	}
}