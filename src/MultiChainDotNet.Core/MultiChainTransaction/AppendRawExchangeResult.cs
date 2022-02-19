using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainTransaction
{
	public class AppendRawExchangeResult
	{
		[JsonProperty("hex")]
		public string Hex { get; set; }

		[JsonProperty("complete")]
		public bool Complete { get; set; }
	}
}
