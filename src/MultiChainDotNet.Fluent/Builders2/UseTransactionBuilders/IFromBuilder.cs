using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders2.UseTransactionBuilders
{
    public interface IFromBuilder
    {
		IFromBuilder Pay(double qty);
		IAnnotateBuilder IssueAsset(UInt64 amt);
		IAnnotateBuilder IssueMoreAsset(string assetName, UInt64 amt);
		IAnnotateBuilder SendAsset(string assetName, double qty);
		IAnnotateBuilder Permit(string permission, string entityName = null);
		IAnnotateBuilder Revoke(string permission, string entityName = null);
		IAnnotateBuilder Filter(string filterName, bool isApprove);
		IAnnotateBuilder Filter(string filterName, string streamName, bool isApprove);
		IAnnotateBuilder UpdateLibrary(string libName, string updateName, bool isApprove);
	}
}
