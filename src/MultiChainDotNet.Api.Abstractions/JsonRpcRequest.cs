namespace MultiChainDotNet.Api.Abstractions
{
	public class JsonRpcRequest
	{
		public string Method { get; set; }
		public object[] Params { get; set; }
	}
}