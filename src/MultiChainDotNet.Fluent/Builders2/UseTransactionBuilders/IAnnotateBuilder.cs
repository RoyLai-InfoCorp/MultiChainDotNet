﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Fluent.Builders2.UseTransactionBuilders
{
    public interface IAnnotateBuilder
    {
		IWithActionBuilder With();
		IWithBuilder AnnotateJson(object json);
		IWithBuilder AnnotateText(string text);
		IWithBuilder AnnotateBytes(byte[] bytes);
	}
}
