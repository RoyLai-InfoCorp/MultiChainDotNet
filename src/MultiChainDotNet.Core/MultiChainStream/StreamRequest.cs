using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainStream
{
	public class StreamRequest
	{
		[JsonProperty("create")]
		public string Create => "stream";

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("open")]
		public bool StreamOpen { get; set; }

		public StreamRequest(string streamName, bool streamOpen)
		{
			Name = streamName;
			StreamOpen = streamOpen;
		}
	}
}
