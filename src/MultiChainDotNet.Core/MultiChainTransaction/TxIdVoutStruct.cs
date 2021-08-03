using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainTransaction
{
    public struct TxIdVoutStruct
    {
		[JsonProperty("txid")]
        public string TxId { get; set; }
		[JsonProperty("vout")]
		public UInt16 Vout { get; set; }
    }
}
