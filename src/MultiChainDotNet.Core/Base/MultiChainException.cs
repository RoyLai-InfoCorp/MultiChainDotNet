// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

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

	}
}
