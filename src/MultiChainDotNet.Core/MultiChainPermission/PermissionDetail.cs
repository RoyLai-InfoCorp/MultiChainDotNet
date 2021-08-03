using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainPermission
{
	public class PermissionDetail
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("streamref")]
		public string StreamRef { get; set; }
	}
}
