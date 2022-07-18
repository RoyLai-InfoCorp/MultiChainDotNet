using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainFilter
{
	public class FilterOptions
	{
		[JsonProperty("for")]
		public List<string> For { get; set; }

		[JsonProperty("libraries")]
		public List<string> Libraries { get; set; }
	}
}
