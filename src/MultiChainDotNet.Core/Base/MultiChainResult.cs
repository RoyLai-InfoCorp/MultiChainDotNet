// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using System;

namespace MultiChainDotNet.Core.Base
{
	public class MultiChainResult
	{
		#region IsError
		public bool IsError => _exception is { };
		#endregion

		#region ErrorCode
		private MultiChainErrorCode _errorCode;
		public MultiChainErrorCode ErrorCode => (MultiChainErrorCode)_errorCode;
		#endregion

		#region Exception
		private Exception _exception;
		public Exception Exception => _exception;
		public string ExceptionMessage => _exception?.Message ?? "";
		#endregion

		public MultiChainResult()
		{
			_errorCode = MultiChainErrorCode.NO_ERROR;
			_exception = null;
		}

		public MultiChainResult(Exception exception)
		{
			_exception = exception;
			_errorCode = exception is MultiChainException ? ((MultiChainException)exception).Code : MultiChainErrorCode.NON_MULTICHAIN_ERROR_CODE;
		}
	}

	public class MultiChainResult<T> : MultiChainResult
	{

		T _result;
		public T Result => _result;

		public MultiChainResult(T result) : base()
		{
			_result = result;
		}

		public MultiChainResult(Exception exception = null) : base(exception)
		{
			_result = default(T);
		}

	}
}
