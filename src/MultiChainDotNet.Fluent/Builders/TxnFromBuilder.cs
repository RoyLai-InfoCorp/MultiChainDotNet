using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders
{
    public class TxnFromBuilder
    {
		private string _fromAddress;

		public TxnFromBuilder(string fromAddress)
		{
			_fromAddress = fromAddress;
		}

		public string Build()
		{
			return _fromAddress;
		}

	}
}
