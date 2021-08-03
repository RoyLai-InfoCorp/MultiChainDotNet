using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainStream
{
	public class StreamItemJsonData
	{
		[JsonProperty("json")]
		public object Json { get; set; }
	}
}
