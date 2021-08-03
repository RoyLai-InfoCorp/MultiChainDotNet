using Newtonsoft.Json;
using System.Collections.Generic;

namespace MultiChainDotNet.Core.MultiChainStream
{
	public class StreamItemsResult
	{
		[JsonProperty("publishers")]
		public List<string> Publishers { get; set; }

		[JsonProperty("keys")]
		public List<string> Keys { get; set; }

		[JsonProperty("data")]
		public object Data { get; set; }

		[JsonProperty("confirmations")]
		public int Confirmations { get; set; }

		[JsonProperty("blockhash")]
		public string BlockHash { get; set; }

		[JsonProperty("blockindex")]
		public int Blockindex { get; set; }

		[JsonProperty("blocktime")]
		public int BlockTime { get; set; }

		[JsonProperty("txid")]
		public string TxId { get; set; }

		[JsonProperty("vout")]
		public int Vout { get; set; }

		[JsonProperty("valid")]
		public bool Valid { get; set; }

		[JsonProperty("time")]
		public long Time { get; set; }

		[JsonProperty("timereceived")]
		public long TimeReceived { get; set; }

		//public StreamItem<T> GetStreamItem<T>()
		//{
		//	object result;
		//	if (typeof(T) == typeof(string))
		//	{
		//		var data = Data.Parse<StreamItemTextData>();
		//		result = data.Text;
		//	}
		//	else
		//	{
		//		var data = Data.Parse<StreamItemJsonData>();
		//		result = data.Json;
		//	}
		//	return new StreamItem<T>
		//	{
		//		Keys = Keys,
		//		Data = result.Parse<T>(),
		//		Publisher = Publishers[0]
		//	};
		//}
	}
}