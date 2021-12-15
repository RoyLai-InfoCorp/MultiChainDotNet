using System;

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
