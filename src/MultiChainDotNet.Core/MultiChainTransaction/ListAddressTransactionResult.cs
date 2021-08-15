using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.MultiChainTransaction
{
	public class ListAddressTransactionResult : ListTransactionResultBase
	{

		[JsonProperty("balance")]
		public ListAddressTransactionBalanceResult Balance { get; set; }

		[JsonProperty("myaddresses")]
		public List<string> MyAddresses { get; set; }

		[JsonProperty("permissions")]
		public List<ListAddressTransactionPermissionsResult> Permissions { get; set; }


	}
}
