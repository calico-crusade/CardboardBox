namespace CardboardBox.Database
{
	public interface ISqlConfig
	{
		string ConnectionString { get; }
		int Timeout { get; }
	}

	public interface ISqlConfig<T> : ISqlConfig { }

	public class SqlConfig<T> : SqlConfig, ISqlConfig<T> { }

	public class SqlConfig : ISqlConfig
	{
		public string ConnectionString { get; set; } = "";
		public int Timeout { get; set; } = 0;
	}
}
