using MultiChainDotNet.Core.MultiChainTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Listener.Service.Controllers
{
	public class TransactionWithId
	{
		public int Id { get; set; }
		public DecodeRawTransactionResult Raw { get; set; }
	}

}
