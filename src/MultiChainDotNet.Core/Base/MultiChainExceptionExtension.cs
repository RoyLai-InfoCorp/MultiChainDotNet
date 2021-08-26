using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.Base
{
    public static class MultiChainExceptionExtension
    {
		public static bool IsMultiChainException(this Exception ex, MultiChainErrorCode code)
		{
			if (ex is MultiChainException)
				if (((MultiChainException)ex).Code == code)
					return true;
			return false;
		}

	}
}
