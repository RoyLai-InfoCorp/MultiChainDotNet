using MultiChainDotNet.Fluent.Signers;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainManagerFactory
	{
		T CreateInstance<T>(SignerBase signer);
		T CreateInstance<T>();
	}
}