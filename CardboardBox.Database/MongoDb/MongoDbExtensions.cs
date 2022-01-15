using MongoDB.Driver;

namespace CardboardBox.Database
{
	public static class MongoDbExtensions
	{
		public static Task<T> Find<T>(this IMongoService<T> service, string id) where T: MongoEntity
		{
			return service.Collection.Find(t => t.Id == id).SingleOrDefaultAsync();
		}

		public static Task<List<T>> Find<T>(this IMongoService<T> service, Func<FilterDefinitionBuilder<T>, FilterDefinition<T>> action)
		{
			var filter = action(service.Filter);
			return service.Collection.Find(filter).ToListAsync();
		}

		public static Task Update<T>(this IMongoService<T> service, T entity) where T: MongoEntity
		{
			return service.Collection.ReplaceOneAsync(
				service.Filter.Eq(t => t.Id, entity.Id),
				entity);
		}

		public static Task<ReplaceOneResult> Upsert<T>(this IMongoService<T> service, T entity) where T: MongoEntity
		{
			return service.Collection.ReplaceOneAsync(
				service.Filter.Eq(t => t.Id, entity.Id),
				entity,
				new ReplaceOptions { IsUpsert = true });
		}

		public static Task Insert<T>(this IMongoService<T> service, T entity) where T: MongoEntity
		{
			return service.Collection.InsertOneAsync(entity);
		}
	}
}
