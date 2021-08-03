using Newtonsoft.Json;

namespace MultiChainDotNet.Core.MultiChainPermission
{
	public class PermissionRequest
	{
		[JsonProperty("for")]
		public string For { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }


		public PermissionRequest(string assetOrStreamName, string permission)
		{
			For = assetOrStreamName;
			Type = permission;
		}
		public PermissionRequest(string permission)
		{
			Type = permission;
		}

	}
}
