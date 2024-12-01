using LiteDB;
using System.Linq.Expressions;
using Woof.Api.DataAccess.Extensions;

namespace Woof.Api.DataAccess;

public class LiteDbContext<T> where T : LiteDbContextEntity
{
    private readonly string _dbPath;
    private readonly string _collection;

    public LiteDbContext(LiteDbConfig liteDbConfig, string collection)
    {
        _dbPath = liteDbConfig.DatabasePath;
        _collection = collection;
    }

    public T Get(Guid id)
    {
        using (var db = new LiteDatabase(_dbPath))
        {
            var collection = db.GetCollection<T>(_collection);
            var result = collection.FindById(id);

            return result;
        }
    }

    public T Get(Query dbQuery)
    {
        using (var db = new LiteDatabase(_dbPath))
        {
            var collection = db.GetCollection<T>(_collection);
            var result = collection.FindOne(dbQuery);

            return result;
        }
    }

    public IEnumerable<T> GetAll()
    {
        using (var db = new LiteDatabase(_dbPath))
        {
            var collection = db.GetCollection<T>(_collection);
            var result = collection.FindAll().ToList();

            return result;
        }
    }
    public T? Find(Expression<Func<T, bool>> predicate)
    {
        using (var db = new LiteDatabase(_dbPath))
        {
            var collection = db.GetCollection<T>(_collection);
            var result = collection.Find(predicate).FirstOrDefault();

            return result;
        }
    }
    public IEnumerable<T> FindAll(Expression<Func<T, bool>> predicate)
    {
        using (var db = new LiteDatabase(_dbPath))
        {
            var collection = db.GetCollection<T>(_collection);
            var result = collection.Find(predicate).ToList();

            return result;
        }
    }

    public Guid Insert(T entity)
    {
        BsonValue result;
        using (var db = new LiteDatabase(_dbPath))
        {
            var collection = db.GetCollection<T>(_collection);
            result = collection.Insert(entity);
        }

        return result.AsGuid;
    }

    public void InsertMany(IEnumerable<T> entity)
    {
        using (var db = new LiteDatabase(_dbPath))
        {
            var collection = db.GetCollection<T>(_collection);
            collection.InsertBulk(entity);
        }
    }
    public void Update(T entity)
    {
        using (var db = new LiteDatabase(_dbPath))
        {
            var collection = db.GetCollection<T>(_collection);
            collection.Update(entity);
        }
    }
}
