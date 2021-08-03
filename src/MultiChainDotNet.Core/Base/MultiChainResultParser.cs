using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace MultiChainDotNet.Core.Base
{
	public static class MultiChainResultParser
	{

		public static MultiChainResult<T> ParseError<T>(dynamic error)
		{
			int code;
			string message;
			try
			{
				code = error.code;
				message = error.message;
				// MultiChain error code is known
				if (Enum.IsDefined(typeof(MultiChainErrorCode), code))
					return new MultiChainResult<T>(new MultiChainException((MultiChainErrorCode)code, message));

			}
			catch
			{
			}
			return new MultiChainResult<T>(
				new MultiChainException(MultiChainErrorCode.UNKNOWN_ERROR_CODE, JsonConvert.SerializeObject(error)));
		}

		public static MultiChainResult<T> ParseResult<T>(dynamic result)
		{
			if (result is JArray || result is JObject)
				return new MultiChainResult<T>(JsonConvert.DeserializeObject<T>(result.ToString()));

			if (result is JValue)
			{
				// If expecting bool
				if (typeof(T) == typeof(bool) && result.Value is bool)
					return new MultiChainResult<T>((T)Convert.ChangeType(result, typeof(bool)));

				// If expecting string
				if (typeof(T) == typeof(string) && result.Value is string)
					return new MultiChainResult<T>((T)Convert.ChangeType(result, typeof(string)));

				// If expecting no return type
				if (typeof(T) == typeof(VoidType))
					return new MultiChainResult<T>();
			}

			return new MultiChainResult<T>(new MultiChainException(MultiChainErrorCode.UNKNOWN_ERROR_CODE, "Expected and return type mistmatch."));
		}

		/// <summary>
		/// MultiChainResult is a wrapper containing the parsed http response result or exception.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="content"></param>
		/// <returns></returns>
		public static MultiChainResult<T> ParseMultiChainResult<T>(string content)
		{
			dynamic json = null;
			bool isJson = false;
			try
			{
				json = JsonConvert.DeserializeObject(content);
				isJson = true;
			}
			catch { }

			try
			{
				if (isJson)
				{
					if (json is bool && typeof(T) == typeof(bool))
						return new MultiChainResult<T>((T)Convert.ChangeType(content, typeof(bool)));

					if (!(json.error is JValue && ((JValue)json.error).Value is null))
						return ParseError<T>(json.error);

					if (json.result is { })
						return ParseResult<T>(json.result);

					return new MultiChainResult<T>(new MultiChainException(MultiChainErrorCode.UNKNOWN_ERROR_CODE, "Result is JSON but schema is invalid."));
				}

				// If expecting string
				if (typeof(T) == typeof(string))
					return new MultiChainResult<T>((T)Convert.ChangeType(content, typeof(string)));

				// If expecting no return type
				if (typeof(T) == typeof(VoidType))
					return new MultiChainResult<T>();

				return new MultiChainResult<T>(new MultiChainException(MultiChainErrorCode.UNKNOWN_ERROR_CODE, "Expected result type returned."));

			}
			catch (Exception ex)
			{
				return new MultiChainResult<T>(new MultiChainException(MultiChainErrorCode.UNKNOWN_ERROR_CODE, ex.Message));
			}

		}

	}
}
