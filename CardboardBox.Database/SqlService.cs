using Dapper;
using System.Data;

namespace CardboardBox.Database
{
	public interface ISqlService<T> : ISqlService where T : IDbConnection, new() { }

	public interface ISqlService
	{
		IDbConnection CreateConnection();

		Task<T> Fetch<T>(string query, object? parameters = null, int? timeout = null, IDbTransaction? transaction = null);
		Task<T[]> Get<T>(string query, object? parameters = null, int? timeout = null, IDbTransaction? transaction = null);
		Task<int> Execute(string query, object? parameters = null, int? timeout = null, IDbTransaction? transaction = null);
		Task<T> ExecuteScalar<T>(string query, object? parameters = null, int? timeout = null, IDbTransaction? transaction = null);
	}

	public class SqlService<T> : SqlService, ISqlService<T> where T : IDbConnection, new()
	{
		private readonly ISqlConfig _config;

		public virtual string ConnectionString => _config.ConnectionString;
		public override int Timeout => _config.Timeout;

		public SqlService(ISqlConfig<T> config)
		{
			_config = config;
		}

		public override IDbConnection CreateConnection()
		{
			var con = Activator.CreateInstance(typeof(T), new object[] { ConnectionString });
			if (con == null) throw new NullReferenceException("Cannot determine connection type");
			return (IDbConnection)con;
		}
	}

	public abstract class SqlService : ISqlService
	{
		public abstract int Timeout { get; }

		public abstract IDbConnection CreateConnection();

		public virtual async Task<T> Fetch<T>(string query, object? parameters = null, int? timeout = null, IDbTransaction? transaction = null)
		{
			using var con = CreateConnection();
			return await con.QueryFirstOrDefaultAsync<T>(query, parameters, transaction, timeout ?? Timeout);
		}

		public virtual async Task<T[]> Get<T>(string query, object? parameters = null, int? timeout = null, IDbTransaction? transaction = null)
		{
			using var con = CreateConnection();
			return (await con.QueryAsync<T>(query, parameters, transaction, timeout ?? Timeout)).ToArray();
		}

		public virtual async Task<int> Execute(string query, object? parameters = null, int? timeout = null, IDbTransaction? transaction = null)
		{
			using var con = CreateConnection();
			return await con.ExecuteAsync(query, parameters, transaction, timeout ?? Timeout);
		}

		public virtual async Task<T> ExecuteScalar<T>(string query, object? parameters = null, int? timeout = null, IDbTransaction? transaction = null)
		{
			using var con = CreateConnection();
			return await con.ExecuteScalarAsync<T>(query, parameters, transaction, timeout ?? Timeout);
		}
	}
}