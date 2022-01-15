using MongoDB.Bson.Serialization.Attributes;

namespace CardboardBox.Database
{
	public abstract class MongoEntity
	{
		[BsonId]
		public string Id { get; set; } = "";
	}
}
