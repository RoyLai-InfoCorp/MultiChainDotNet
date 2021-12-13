// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using Microsoft.Extensions.Logging;
using MultiChainDotNet.Core.MultiChainTransaction;
using System.Collections.Generic;

namespace MultiChainDotNet.Fluent.Builders
{
	public enum LibraryUpdateMode { NONE, INSTANT, APPROVE }

	public interface ITransactionBuilder
	{
		#region Common
		ITransactionBuilder AddLogger(ILogger logger);
		string Describe();
		string CreateRawTransaction(MultiChainTransactionCommand txnCmd);

		#endregion

		#region From
		ITransactionBuilder From(string address);
		ITransactionBuilder From(string txid, ushort vout);
		#endregion

		#region To
		ITransactionBuilder Pay(double qty);
		ITransactionBuilder To(string address);
		ITransactionBuilder IssueAsset(ulong amt);
		ITransactionBuilder IssueMoreAsset(string assetName, ulong amt);
		ITransactionBuilder SendAsset(string assetName, double qty);
		ITransactionBuilder Permit(string permission, string entityName = null);
		ITransactionBuilder Revoke(string permission, string entityName = null);
		ITransactionBuilder AnnotateBytes(byte[] bytes);
		ITransactionBuilder AnnotateJson(object json);
		ITransactionBuilder AnnotateText(string text);
		ITransactionBuilder Filter(string filterName, bool isApprove);
		ITransactionBuilder Filter(string filterName, string streamName, bool isApprove);
		ITransactionBuilder UpdateLibrary(string libName, string updateName, bool isApprove);

		#endregion

		#region With
		ITransactionBuilder With();
		ITransactionBuilder DeclareBytes(byte[] bytes);
		ITransactionBuilder DeclareJson(object json);
		ITransactionBuilder DeclareText(string text);
		ITransactionBuilder IssueDetails(string assetName, uint multiple, bool canIssueMore);
		ITransactionBuilder IssueDetails(string assetName, uint multiple, bool reissuable, Dictionary<string, object> details);
		ITransactionBuilder IssueMoreDetails(string assetName);
		ITransactionBuilder IssueMoreDetails(string assetName, Dictionary<string, object> details);
		ITransactionBuilder IssueNonFungibleAsset(string nfa);
		ITransactionBuilder IssueToken(string nfaName, string tokenId, int qty);
		ITransactionBuilder SendToken(string nfaName, string tokenId, int qty);
		ITransactionBuilder CreateStream(string streamName, bool anyoneCanWrite);
		ITransactionBuilder CreateStream(string streamName, bool publicWritable, Dictionary<string, object> details);
		ITransactionBuilder CreateVariable(string variableName, object value);
		ITransactionBuilder PublishJson(string streamName, string key, object json);
		ITransactionBuilder PublishJson(string streamName, string[] keys, object json);
		ITransactionBuilder PublishText(string streamName, string key, string text);
		ITransactionBuilder UpdateJavascript(string scriptName, string versionName, string javascript);
		ITransactionBuilder UpdateVariable(string variableName, object value);
		ITransactionBuilder AddJavascript(string scriptName, LibraryUpdateMode mode, string javascript);

		#endregion

		IMultiSigTransactionBuilder UseMultiSigTransaction(MultiChainTransactionCommand txnCmd);
		IMultiSigTransactionBuilder CreateMultiSigTransaction(MultiChainTransactionCommand txnCmd);

		INormalTransactionBuilder UseNormalTransaction(MultiChainTransactionCommand txnCmd);
		INormalTransactionBuilder CreateNormalTransaction(MultiChainTransactionCommand txnCmd);


	}
}

