using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Base
{
    public class Txn
    {
		public string Version { get; set; }
		public ushort TxInCount { get; set; }
		public TxIn[] Inputs { get; set; }
		public ushort TxOutCount { get; set; }
		public TxOut[] Outputs { get; set; }
		public string LockTime { get; set; }
	}
}
