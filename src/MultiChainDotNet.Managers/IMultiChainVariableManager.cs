using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Fluent.Signers;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainVariableManager
	{
		MultiChainResult<string> CreateVariable(string variableName, SignerBase signer = null);
		Task<MultiChainResult<T>> GetVariableValueAsync<T>(string variableName);
		MultiChainResult<string> SetVariableValue(string variableName, object variableValue, SignerBase signer = null);
	}
}