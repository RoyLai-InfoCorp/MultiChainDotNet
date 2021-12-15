// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Core.Base;
using MultiChainDotNet.Fluent.Signers;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainVariableManager
	{
		MultiChainResult<string> CreateVariable(SignerBase signer, string variableName);
		MultiChainResult<string> CreateVariable(string variableName);
		Task<MultiChainResult<T>> GetVariableValueAsync<T>(string variableName);
		MultiChainResult<string> SetVariableValue<T>(SignerBase signer, string variableName, T variableValue);
		MultiChainResult<string> SetVariableValue<T>(string variableName, T variableValue);

	}
}
