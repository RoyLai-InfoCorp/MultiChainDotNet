using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainDotNet.Tests.IntegrationTests
{
	public class TestStateDb
	{
		string dbname = "state.db";

		public T GetState<T>()
		{
			using (LiteDatabase db = new LiteDatabase(dbname))
			{
				var list = db.GetCollection<T>();
				return list.FindAll().FirstOrDefault();
			}
		}

		public void SaveState<T>(T state)
		{
			using (LiteDatabase db = new LiteDatabase(dbname))
			{
				var list = db.GetCollection<T>();
				list.DeleteAll();
				list.Upsert(state);
			}
		}

		public void ClearState<T>()
		{
			using (LiteDatabase db = new LiteDatabase(dbname))
			{
				var list = db.GetCollection<T>();
				list.DeleteAll();
			}
		}

	}
}
