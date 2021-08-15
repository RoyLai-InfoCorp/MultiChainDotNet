using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders2.UseTransactionBuilders
{
    public interface IDeclarationBuilder
    {
		IRequestBuilder DeclareBytes(byte[] bytes);
		IRequestBuilder DeclareJson(object json);
		IRequestBuilder DeclareText(string text);
	}
}
