using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Core.Utils
{
    public class TaskHelper
    {
		public static async Task WaitUntilTrue(Func<Task<bool>> func, int retries, int delay)
		{
			var isTrue = await func();
			for (int i = 0; i < retries; i++)
			{
				await Task.Delay(delay);
				isTrue = await func();
				if (isTrue)
					return;
			}
			throw new Exception($"Wait for true timeout: {nameof(func)}");
		}

	}
}
