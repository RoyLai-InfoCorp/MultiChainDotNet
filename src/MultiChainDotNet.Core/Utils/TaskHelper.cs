using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.Utils
{
    public class TaskHelper
    {
		public static async Task<bool> WaitUntilTrue(Func<Task<bool>> func, int retries = 5, int delay = 500)
		{
			var isTrue = await func();
			for (int i = 0; i < retries; i++)
			{
				await Task.Delay(delay);
				isTrue = await func();
				if (isTrue)
					return true;
			}
			return false;
		}

	}
}
