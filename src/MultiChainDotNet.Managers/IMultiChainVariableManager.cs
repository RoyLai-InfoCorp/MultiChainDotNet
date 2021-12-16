// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using MultiChainDotNet.Fluent.Signers;
using System.Threading.Tasks;

namespace MultiChainDotNet.Managers
{
	public interface IMultiChainVariableManager
	{
		string CreateVariable(SignerBase signer, string variableName);
		string CreateVariable(string variableName);
		Task<T> GetVariableValueAsync<T>(string variableName);
		string SetVariableValue<T>(SignerBase signer, string variableName, T variableValue);
		string SetVariableValue<T>(string variableName, T variableValue);
	}
}