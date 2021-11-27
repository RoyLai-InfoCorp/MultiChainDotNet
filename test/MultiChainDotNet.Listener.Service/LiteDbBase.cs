using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

public class LiteDbBase<T>
{
	private string _db;
	protected string Db => _db;

	protected string _collectionName = typeof(T).Name;

	public LiteDbBase(string db)
	{
		_db = $"Filename={db}.db; Connection=shared";
		// Custom Mapping required for BigInteger
		BsonMapper.Global.RegisterType<BigInteger>
		(
			serialize: (bigInt) => bigInt.ToString(),
			deserialize: (bson) => BigInteger.Parse(bson.AsString)
		);
	}

	public virtual ILiteCollection<T> GetCollection(LiteDatabase db)
	{
		return db.GetCollection<T>(_collectionName);
	}

	public virtual Task Upsert(T package)
	{
		using (var db = new LiteDatabase(Db))
		{
			var col = db.GetCollection<T>(_collectionName);
			col.Upsert(package);
		}
		return Task.CompletedTask;
	}

	public virtual Task Insert(T package)
	{
		using (var db = new LiteDatabase(Db))
		{
			var col = db.GetCollection<T>(_collectionName);
			col.Insert(package);
		}
		return Task.CompletedTask;
	}

	public virtual Task Update(T package)
	{
		using (var db = new LiteDatabase(Db))
		{
			var col = db.GetCollection<T>(_collectionName);
			col.Update(package);
		}
		return Task.CompletedTask;
	}

	public virtual Task<T> GetById(string id)
	{
		using (var db = new LiteDatabase(Db))
		{
			var col = db.GetCollection<T>(_collectionName);
			return Task.FromResult((T)col.FindById(new BsonValue(id)));
		}
	}

	public virtual Task Delete(string id)
	{
		using (var db = new LiteDatabase(Db))
		{
			var col = db.GetCollection<T>(_collectionName);
			col.Delete(new BsonValue(id));
		}
		return Task.CompletedTask;
	}

	public Task DeleteAll()
	{
		using (var db = new LiteDatabase(Db))
		{
			var col = db.GetCollection<T>(_collectionName);
			col.DeleteAll();
		}
		return Task.CompletedTask;
	}

	public Task<IList<T>> ListAll()
	{
		using (var db = new LiteDatabase(Db))
		{
			var col = db.GetCollection<T>(_collectionName);
			return Task.FromResult((IList<T>)col.FindAll().ToList());
		}
	}

	public Task Dump(string filePath)
	{
		using (var db = new LiteDatabase(Db))
		{
			var col = db.GetCollection<T>(_collectionName);
			File.Delete(filePath);
			db.Execute($"SELECT $ INTO $FILE('{filePath}') FROM {_collectionName}");
		}
		return Task.CompletedTask;
	}

}
