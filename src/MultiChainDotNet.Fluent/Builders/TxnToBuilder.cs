using MultiChainDotNet.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders
{
    public class TxnToBuilder
    {
		private string _addressTo;
		private Dictionary<string, object> _itemsTo = new Dictionary<string, object>();

		public TxnToBuilder(string addressTo)
		{
			_addressTo = addressTo;
		}

		//---- BUILDER FUNCTION

		public (string AddressTo, Dictionary<string,object> ItemsTo) Build()
		{
			return (_addressTo, _itemsTo);
		}

		//---- MULTICHAIN FUNCTION

		public TxnToBuilder Pay(double qty)
		{
			_itemsTo[""] = qty;
			return this;
		}

		public TxnToBuilder IssueAsset(UInt64 amt)
		{
			_itemsTo["issue"] = new { raw = amt };
			return this;
		}

		//public TxnToBuilder IssueAsset(double qty, UInt32 multiple)
		//{
		//	return IssueAsset(Convert.ToUInt64(qty * multiple));
		//}

		public TxnToBuilder IssueMoreAsset(string assetName, UInt64 amt)
		{
			_itemsTo["issuemore"] = new { asset = assetName, raw = amt };
			return this;
		}

		//public TxnToBuilder IssueMoreAsset(string assetName, double qty, UInt32 multiple)
		//{
		//	return IssueMoreAsset(assetName, Convert.ToUInt64(qty * multiple));
		//}

		public TxnToBuilder SendAsset(string assetName, double qty)
		{
			_itemsTo[assetName] = qty;
			return this;
		}

		public TxnToBuilder Permit(string permission, string entityName = null)
		{
			//_itemsTo["permissions"] = new Dictionary<string,object> { { "type", permission }, { "for", entityName } };
			_itemsTo["permissions"] = new Dictionary<string, object> { { "type", permission } };
			if (entityName is { })
				((Dictionary<string, object>)_itemsTo["permissions"])["for"]=entityName;

			return this;
		}

		public TxnToBuilder Revoke(string permission, string entityName = null)
		{
			//_itemsTo["permissions"] = new Dictionary<string, object> { { "type", permission }, { "for", entityName }, { "startblock",0 }, { "endblock", 0 } };
			_itemsTo["permissions"] = new Dictionary<string, object> { { "type", permission }, { "startblock", 0 }, { "endblock", 0 } };
			if (entityName is { })
				((Dictionary<string, object>)_itemsTo["permissions"])["for"] = entityName;

			return this;
		}

		public TxnToBuilder AnnotateJson(object json)
		{
			if (json is { })
				_itemsTo["data"] = new Dictionary<string, object> { { "json", json } };
			return this;
		}

		public TxnToBuilder AnnotateText(string text)
		{
			if (!String.IsNullOrEmpty(text))
				_itemsTo["data"] = new Dictionary<string, string> { { "text", text } };
			return this;
		}

		public TxnToBuilder AnnotateBytes(byte[] bytes)
		{
			if (bytes is { })
				_itemsTo["data"] = new Dictionary<string, string> { { "cache", bytes.Bytes2Hex() } };
			return this;
		}

		public TxnToBuilder Filter(string filterName, bool isApprove)
		{
			_itemsTo[filterName] = new { approve = isApprove };
			return this;
		}

		public TxnToBuilder Filter(string filterName, string streamName, bool isApprove)
		{
			_itemsTo[filterName] = new Dictionary<string, string> { { "approve", isApprove.ToString() }, { "for", streamName} };
			return this;
		}

		public TxnToBuilder UpdateLibrary(string libName, string updateName, bool isApprove)
		{
			_itemsTo[libName] = new Dictionary<string, string> { { "approve", isApprove.ToString() }, { "updatename", updateName } };
			return this;
		}

	}
}
