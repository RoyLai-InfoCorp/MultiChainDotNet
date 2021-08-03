using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainStream
{
	public class StreamItemTextData
	{
		[JsonProperty("text")]
		public string Text { get; set; }
	}
}
