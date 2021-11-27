using LiteDB;
using MultiChainDotNet.Core.MultiChainTransaction;
using System.Text.Encodings.Web;
using System.Text.Json;

public class RawTransactionsRepository : LiteDbBase<DecodeRawTransactionResult>
{

	public RawTransactionsRepository(string db) : base(db)
	{
		var jso = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

		BsonMapper.Global.RegisterType<object>
		(
			serialize: (input) =>
			{
				return System.Text.Json.JsonSerializer.Serialize(input, jso);
			},
			deserialize: (output) =>
			{
				return System.Text.Json.JsonSerializer.Deserialize<object>(output.AsString, jso);
			}
		);

		//BsonMapper.Global.RegisterType<object>
		//(
		//	serialize: (input) =>
		//	{
		//		var serialized = System.Text.Json.JsonSerializer.Serialize(input);
		//		return serialized;
		//	},
		//	deserialize: (output) =>
		//	{
		//		var result = output.AsString;
		//		var deserialized = System.Text.Json.JsonDocument.Parse(output.AsString);
		//		return deserialized.RootElement;
		//	}
		//);

	}
}
