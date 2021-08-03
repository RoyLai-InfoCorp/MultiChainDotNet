using MultiChainDotNet.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders
{
    public class TxnWithBuilder
    {
		List<object> _withData = new List<object>();
        public List<object> Build()
		{
			return _withData;
		}

		public TxnWithBuilder DeclareBytes(byte[] bytes)
		{
			if (bytes is { })
				_withData.Add(bytes.Bytes2Hex());
			return this;
		}

		public TxnWithBuilder DeclareJson(object json)
		{
			if (json is { })
				_withData.Add(new { json = json });
			return this;
		}

		public TxnWithBuilder DeclareText(string text)
		{
			if (!String.IsNullOrEmpty(text))
			_withData.Add(new { text = text });
			return this;
		}

		public TxnWithBuilder IssueDetails(string assetName, UInt32 multiple, bool canIssueMore)
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

		public TxnWithBuilder IssueDetails(string assetName, UInt32 multiple, bool reissuable, Dictionary<string, object> details)
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


		public TxnWithBuilder IssueMoreDetails(string assetName)
		{
			_withData.Add(
				new
				{
					update = assetName
				});
			return this;
		}
		public TxnWithBuilder IssueMoreDetails(string assetName, Dictionary<string, object> details)
		{
			_withData.Add(
				new
				{
					update = assetName,
					details = details
				});
			return this;
		}

		public TxnWithBuilder CreateStream(string streamName, bool publicWritable, Dictionary<string, object> details)
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


		public TxnWithBuilder CreateStream(string streamName, bool anyoneCanWrite)
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

		public TxnWithBuilder PublishJson(string streamName, string key, object json)
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

		public TxnWithBuilder PublishJson(string streamName, string[] keys, object json)
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


		public TxnWithBuilder PublishText(string streamName, string key, string text)
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

		public TxnWithBuilder CreateVariable(string variableName, object value)
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

		public TxnWithBuilder UpdateVariable(string variableName, object value)
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
		public TxnWithBuilder AddJavascript(string scriptName, LibraryUpdateMode mode, string javascript)
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

		public TxnWithBuilder UpdateJavascript(string scriptName, string versionName, string javascript)
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
