using Newtonsoft.Json;

namespace MultiChainDotNet.Core.Utils
{
	public static class ObjectExtensions
	{
		public static string ToJson(this object obj)
		{
			return JsonConvert.SerializeObject(obj,
				Formatting.Indented,
				new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
		}

	}
}
