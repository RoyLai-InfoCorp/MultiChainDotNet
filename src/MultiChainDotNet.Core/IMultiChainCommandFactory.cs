using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainBlockchain;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.MultiChainTransaction;

namespace MultiChainDotNet.Core
{
	public interface IMultiChainCommandFactory
	{
		T CreateCommand<T>() where T : MultiChainCommandBase;
	}
}