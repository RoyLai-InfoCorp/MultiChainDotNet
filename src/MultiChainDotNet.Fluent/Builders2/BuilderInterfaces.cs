using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Fluent.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiChainDotNet.Fluent.Builders2.TxnWithBuilder;

namespace MultiChainDotNet.Fluent.Builders2
{
	public interface IFromBuilder
	{
		IToBuilder From(string toAddress);
	}

	public interface IToBuilder
	{
		IToActionBuilder To(string toAddress);
		IWithActionBuilder With();
	}

	public interface IToActionBuilder
	{
		IToActionBuilder Pay(double qty);
		IAnnotateBuilder IssueAsset(UInt64 amt);
		IAnnotateBuilder IssueMoreAsset(string assetName, UInt64 amt);
		IAnnotateBuilder SendAsset(string assetName, double qty);
		IAnnotateBuilder Permit(string permission, string entityName = null);
		IAnnotateBuilder Revoke(string permission, string entityName = null);
		IAnnotateBuilder Filter(string filterName, bool isApprove);
		IAnnotateBuilder Filter(string filterName, string streamName, bool isApprove);
		IAnnotateBuilder UpdateLibrary(string libName, string updateName, bool isApprove);
		IWithActionBuilder With();
		IAddSignerBuilder CreateTransaction(MultiChainTransactionCommand txnCmd);
		IAddMultiSigSenderBuilder CreateMultiSigTransaction(MultiChainTransactionCommand txnCmd);
		string CreateRawTransaction(MultiChainTransactionCommand txnCmd);
	}

	public interface IAnnotateBuilder
	{
		IWithActionBuilder With();
		IWithBuilder AnnotateJson(object json);
		IWithBuilder AnnotateText(string text);
		IWithBuilder AnnotateBytes(byte[] bytes);
		IAddSignerBuilder CreateTransaction(MultiChainTransactionCommand txnCmd);
		IAddMultiSigSenderBuilder CreateMultiSigTransaction(MultiChainTransactionCommand txnCmd);
		string CreateRawTransaction(MultiChainTransactionCommand txnCmd);
	}

	public interface IWithBuilder
	{
		IWithActionBuilder With();
	}

	public interface IWithActionBuilder
	{
		IDeclarationBuilder IssueDetails(string assetName, UInt32 multiple, bool canIssueMore);
		IDeclarationBuilder IssueDetails(string assetName, UInt32 multiple, bool reissuable, Dictionary<string, object> details);
		IDeclarationBuilder IssueMoreDetails(string assetName);
		IDeclarationBuilder CreateStream(string streamName, bool publicWritable, Dictionary<string, object> details);
		IDeclarationBuilder CreateStream(string streamName, bool anyoneCanWrite);
		IDeclarationBuilder PublishJson(string streamName, string key, object json);
		IDeclarationBuilder PublishJson(string streamName, string[] keys, object json);
		IDeclarationBuilder PublishText(string streamName, string key, string text);
		IDeclarationBuilder CreateVariable(string variableName, object value);
		IDeclarationBuilder UpdateVariable(string variableName, object value);
		IDeclarationBuilder AddJavascript(string scriptName, LibraryUpdateMode mode, string javascript);
		IDeclarationBuilder UpdateJavascript(string scriptName, string versionName, string javascript);
		IAddSignerBuilder CreateNormalTransaction(MultiChainTransactionCommand txnCmd);
		IAddMultiSigSenderBuilder CreateMultiSigTransaction(MultiChainTransactionCommand txnCmd);
		string CreateRawTransaction(MultiChainTransactionCommand txnCmd);
	}

	public interface IDeclarationBuilder
	{
		IRequestBuilder DeclareBytes(byte[] bytes);
		IRequestBuilder DeclareJson(object json);
		IRequestBuilder DeclareText(string text);
		IAddSignerBuilder CreateNormalTransaction(MultiChainTransactionCommand txnCmd);
		IAddMultiSigSenderBuilder CreateMultiSigTransaction(MultiChainTransactionCommand txnCmd);
		string CreateRawTransaction(MultiChainTransactionCommand txnCmd);
	}

	public interface IRequestBuilder
	{
		IAddSignerBuilder CreateNormalTransaction(MultiChainTransactionCommand txnCmd);
		IAddMultiSigSenderBuilder CreateMultiSigTransaction(MultiChainTransactionCommand txnCmd);
		string CreateRawTransaction(MultiChainTransactionCommand txnCmd);
	}

	public interface IAddSignerBuilder
	{
		IAddSignerBuilder AddSigner(SignerBase signer);
		ISignBuilder Sign();
	}

	public interface ISignBuilder
	{
		string RawSigned();
		string Send();
	}

	public interface IAddMultiSigSenderBuilder
	{
		IAddMultiSigSenderBuilder AddSigner(SignerBase signer);
		ISignBuilder MultiSign(string redeemScript);
		ISignBuilder MultiSign(string redeemScript, IList<string[]> signersSignatures);
		string CreateMultiSigTransactionHashes(string redeemScript);
	}


}
