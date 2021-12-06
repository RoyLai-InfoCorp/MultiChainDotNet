namespace MultiChainDotNet.Api.Service.Controllers
{
	public class JsonRpcRequest
	{
		public string Method { get; set; }
		public object[] Params { get; set; }
	}
}