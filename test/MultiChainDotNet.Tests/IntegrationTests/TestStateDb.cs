// SPDX-FileCopyrightText: 2020-2021 InfoCorp Technologies Pte. Ltd. <roy.lai@infocorp.io>
// SPDX-License-Identifier: See LICENSE.txt

using LiteDB;
using System.Linq;

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
