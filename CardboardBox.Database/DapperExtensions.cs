using Dapper;

namespace CardboardBox.Database
{
	public static class DapperExtensions
	{
		public static async Task<(T1 item1, T2 item2, T3 item3, T4 item4)[]> QueryAsync<T1, T2, T3, T4>(this ISqlService sql, string query, object? parameters = null, string splitOn = "split")
		{
			using var con = sql.CreateConnection();
			return (await con.QueryAsync<T1, T2, T3, T4, (T1, T2, T3, T4)>(query,
				(a, b, c, d) => (a, b, c, d),
				parameters,
				splitOn: splitOn)).ToArray();
		}

		public static async Task<(T1 item1, T2 item2, T3 item3)[]> QueryAsync<T1, T2, T3>(this ISqlService sql, string query, object? parameters = null, string splitOn = "split")
		{
			using var con = sql.CreateConnection();
			return (await con.QueryAsync<T1, T2, T3, (T1, T2, T3)>(query,
				(a, b, c) => (a, b, c),
				parameters,
				splitOn: splitOn)).ToArray();
		}

		public static async Task<(T1 item1, T2 item2)[]> QueryAsync<T1, T2>(this ISqlService sql, string query, object? parameters = null, string splitOn = "split")
		{
			using var con = sql.CreateConnection();
			return (await con.QueryAsync<T1, T2, (T1, T2)>(query,
				(a, b) => (a, b),
				parameters,
				splitOn: splitOn)).ToArray();
		}
	}
}
