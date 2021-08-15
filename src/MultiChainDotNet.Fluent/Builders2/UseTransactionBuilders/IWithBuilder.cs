using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders2.UseTransactionBuilders
{
    public interface IWithBuilder
    {
		IDeclarationBuilder IssueDetails(string assetName, UInt32 multiple, bool canIssueMore);
		IDeclarationBuilder IssueDetails(string assetName, UInt32 multiple, bool reissuable, Dictionary<string, object> details);
		IDeclarationBuilder IssueMoreDetails(string assetName);
		IDeclarationBuilder CreateStream(string streamName, bool publicWritable, Dictionary<string, object> details);
		IDeclarationBuilder CreateStream(string streamName, bool anyoneCanWrite);
		IDeclarationBuilder PublishJson(string streamName, string key, object json);
		IDeclarationBuilder PublishJson(string streamName, string[] keys, object json);
		IDeclarationBuilder PublishText(string streamName, string key, string text);
		IDeclarationBuilder CreateVariable(string variableName, object value);
		IDeclarationBuilder UpdateVariable(string variableName, object value);
		IDeclarationBuilder AddJavascript(string scriptName, int mode, string javascript);
		IDeclarationBuilder UpdateJavascript(string scriptName, string versionName, string javascript);

	}
}
