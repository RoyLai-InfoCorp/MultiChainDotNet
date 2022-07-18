using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainFilter
{
	public class TestFilterResult
	{
		[JsonProperty("compiled")]
		public bool Compiled { get; set; }

		[JsonProperty("reason")]
		public string Reason { get; set; }

		[JsonProperty("passed")]
		public bool Passed { get; set; } = true;

		[JsonProperty("callbacks")]
		public object[] Callbacks { get; set; }

		[JsonProperty("time")]
		public double Time { get; set; }
	}
}
