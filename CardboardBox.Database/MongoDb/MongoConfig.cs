namespace CardboardBox.Database
{
	public interface IMongoConfig
	{
		string ConnectionString { get; }
		string DatabaseName { get; }
		string CollectionName { get; }
	}

	public class MongoConfig : IMongoConfig
	{
		public string ConnectionString { get; set; } = "";
		public string DatabaseName { get; set; } = "";
		public string CollectionName { get; set; } = "";
	}
}
