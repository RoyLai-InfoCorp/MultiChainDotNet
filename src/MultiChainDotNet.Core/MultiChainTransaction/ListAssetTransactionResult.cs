using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainTransaction
{
    public class ListAssetTransactionResult : ListTransactionResultBase
    {

		[JsonProperty("addresses")]
		public Dictionary<string,Int64> AddressTransferredQty { get; set; }

	}
}
