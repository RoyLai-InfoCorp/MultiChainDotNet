using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainFilter
{
	public class TxFilterItem
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("createtxid")]
		public string CreateTxid { get; set; }

		[JsonProperty("filterref")]
		public string FilterRef { get; set; }

		[JsonProperty("language")]
		public string Language { get; set; }

		[JsonProperty("codelength")]
		public UInt64 CodeLength { get; set; }

		[JsonProperty("for")]
		public string[] For { get; set; }

		[JsonProperty("libraries")]
		public string[] Libraries { get; set; }

		[JsonProperty("compiled")]
		public bool Compiled { get; set; }

		[JsonProperty("approved")]
		public bool Approved { get; set; }

	}
}
