using MultiChainDotNet.Core.MultiChainTransaction;
using MultiChainDotNet.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders2
{
    public class TxnWithBuilder : IWithActionBuilder, IDeclarationBuilder, IRequestBuilder
	{
		List<object> _withData = new List<object>();
		MultiChainFluentApi _fluent;

		public TxnWithBuilder(MultiChainFluentApi fluent)
		{
			_fluent = fluent;
		}

        public List<object> Build()
		{
			return _withData;
		}

		public IAddSignerBuilder CreateTransaction(MultiChainTransactionCommand txnCmd)
		{
			return _fluent.CreateTransaction(txnCmd);
		}

		public IAddMultiSigSenderBuilder CreateMultiSigTransaction(MultiChainTransactionCommand txnCmd)
		{
			return _fluent.CreateMultiSigTransaction(txnCmd);
		}
		public string CreateRawTransaction(MultiChainTransactionCommand txnCmd)
		{
			return _fluent.CreateRawTransaction(txnCmd);
		}


		public IRequestBuilder DeclareBytes(byte[] bytes)
		{
			if (bytes is { })
				_withData.Add(bytes.Bytes2Hex());
			return this;
		}

		public IRequestBuilder DeclareJson(object json)
		{
			if (json is { })
				_withData.Add(new { json = json });
			return this;
		}

		public IRequestBuilder DeclareText(string text)
		{
			if (!String.IsNullOrEmpty(text))
			_withData.Add(new { text = text });
			return this;
		}

		public IDeclarationBuilder IssueDetails(string assetName, UInt32 multiple, bool canIssueMore)
		{
			_withData.Add(
				new
				{
					create = "asset",
					name = assetName,
					multiple = multiple,
					open = canIssueMore
				});
			return this;
		}

		public IDeclarationBuilder IssueDetails(string assetName, UInt32 multiple, bool reissuable, Dictionary<string, object> details)
		{
			_withData.Add(
				new
				{
					create = "asset",
					name = assetName,
					multiple = multiple,
					open = reissuable,
					details = details
				});
			return this;
		}


		public IDeclarationBuilder IssueMoreDetails(string assetName)
		{
			_withData.Add(
				new
				{
					update = assetName
				});
			return this;
		}
		public IDeclarationBuilder IssueMoreDetails(string assetName, Dictionary<string, object> details)
		{
			_withData.Add(
				new
				{
					update = assetName,
					details = details
				});
			return this;
		}

		public IDeclarationBuilder CreateStream(string streamName, bool publicWritable, Dictionary<string, object> details)
		{
			_withData.Add(
				new
				{
					create = "stream",
					name = streamName,
					open = publicWritable,
					details = details
				});
			return this;
		}


		public IDeclarationBuilder CreateStream(string streamName, bool anyoneCanWrite)
		{
			_withData.Add(
				new
				{
					create = "stream",
					name = streamName,
					open = anyoneCanWrite
				});
			return this;
		}

		public IDeclarationBuilder PublishJson(string streamName, string key, object json)
		{
			_withData.Add(
				new Dictionary<string,object>
				{
					{ "for" , streamName },
					{ "key" , key },
					{ "data", new { json = json } }
				});
			return this;
		}

		public IDeclarationBuilder PublishJson(string streamName, string[] keys, object json)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "for" , streamName },
					{ "keys" , keys },
					{ "data", new { json = json } }
				});
			return this;
		}


		public IDeclarationBuilder PublishText(string streamName, string key, string text)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "for" , streamName },
					{ "key" , key },
					{ "data", new { text = text } }
				});
			return this;
		}

		public IDeclarationBuilder CreateVariable(string variableName, object value)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "create" , "variable" },
					{ "name" , variableName },
					{ "value", value }
				});
			return this;
		}

		public IDeclarationBuilder UpdateVariable(string variableName, object value)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "update" , variableName },
					{ "value", value }
				});
			return this;
		}

		public enum LibraryUpdateMode { NONE, INSTANT, APPROVE }
		public IDeclarationBuilder AddJavascript(string scriptName, LibraryUpdateMode mode, string javascript)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "create" , "library" },
					{ "name" , scriptName },
					{ "updatemode" , mode.ToString().ToLower()},
					{ "code", javascript }
				});
			return this;
		}

		public IDeclarationBuilder UpdateJavascript(string scriptName, string versionName, string javascript)
		{
			_withData.Add(
				new Dictionary<string, object>
				{
					{ "update" , scriptName },
					{ "updatename" , versionName },
					{ "code", javascript }
				});
			return this;
		}


	}
}
