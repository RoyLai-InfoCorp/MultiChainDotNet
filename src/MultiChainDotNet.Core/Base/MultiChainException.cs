using Humanizer;
using System;

namespace MultiChainDotNet.Core.Base
{
	public class MultiChainException : Exception
	{
		public MultiChainErrorCode Code { get; }

		public MultiChainException(MultiChainErrorCode code) : base(
			$"Error code:{(int)code}({code.ToString().Humanize()})")
		{
			Code = code;
		}

		public MultiChainException(MultiChainErrorCode code, string message) : base(
			$"Error code:{(int)code}({code.ToString().Humanize()})\nMessage: {message}.")
		{
			Code = code;
		}

		public static bool IsException(Exception ex, MultiChainErrorCode code)
		{
			if (ex is MultiChainException)
				if (((MultiChainException)ex).Code == code)
					return true;
			return false;
		}
	}
}
