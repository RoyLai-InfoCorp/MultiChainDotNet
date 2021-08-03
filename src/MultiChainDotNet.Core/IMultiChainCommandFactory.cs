using MultiChainDotNet.Core.MultiChainAddress;
using MultiChainDotNet.Core.MultiChainAsset;
using MultiChainDotNet.Core.MultiChainPermission;
using MultiChainDotNet.Core.MultiChainStream;
using MultiChainDotNet.Core.MultiChainTransaction;

namespace MultiChainDotNet.Core
{
	public interface IMultiChainCommandFactory
	{
		MultiChainAddressCommand CreateMultiChainAddressCommand();
		MultiChainAssetCommand CreateMultiChainAssetCommand();
		MultiChainPermissionCommand CreateMultiChainPermissionCommand();
		MultiChainStreamCommand CreateMultiChainStreamCommand();
		MultiChainTransactionCommand CreateMultiChainTransactionCommand();
	}
}