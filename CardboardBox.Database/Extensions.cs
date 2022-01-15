using Dapper.FluentMap;
using Dapper.FluentMap.Configuration;
using Dapper.FluentMap.Conventions;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace CardboardBox.Database
{
	public static class Extensions
	{
		public static IServiceCollection DapperCamelCase(this IServiceCollection service, Action<FluentConventionConfiguration>? config = null)
		{
			return DapperFluentMap<CamelCaseMap>(service, config);
		}

		public static IServiceCollection DapperFluentMap<T>(this IServiceCollection service, Action<FluentConventionConfiguration>? config = null) where T: Convention, new()
		{
			return DapperFluentMap(service, t =>
			{
				var a = t.AddConvention<T>();
				config?.Invoke(a);
			});
		}

		public static IServiceCollection DapperFluentMap(this IServiceCollection services, Action<FluentMapConfiguration> config)
		{
			FluentMapper.Initialize(c =>
			{
				config(c);
			});
			return services; 
		}

		public static IServiceCollection AddSqlService<T, TConfig>(this IServiceCollection services, bool moreThanOne = false) 
			where T : IDbConnection, new()
			where TConfig : class, ISqlConfig<T>, new()
		{
			if (moreThanOne)
				return services
					.AddTransient<ISqlService<T>, SqlService<T>>()
					.AddTransient<ISqlConfig<T>, TConfig>();

			return services
				.AddTransient<ISqlService, SqlService<T>>()
				.AddTransient<ISqlConfig<T>, TConfig>();
		}

		public static IServiceCollection AddSqlService<T>(this IServiceCollection services, ISqlConfig config, bool moreThanOne = false) where T : IDbConnection, new()
		{
			var scopedConfig = new SqlConfig<T>
			{
				ConnectionString = config.ConnectionString,
				Timeout = config.Timeout
			};

			if (moreThanOne)
				return services
					.AddTransient<ISqlService<T>, SqlService<T>>()
					.AddSingleton<ISqlConfig<T>>(scopedConfig);

			return services
				.AddTransient<ISqlService, SqlService<T>>()
				.AddSingleton<ISqlConfig<T>>(scopedConfig);
		}

		public static IServiceCollection AddSqlService<T>(this IServiceCollection services, string connectionString, int timeout = 0, bool moreThanOne = false) where T : IDbConnection, new()
		{
			return AddSqlService<T>(services, new SqlConfig<T>
			{
				ConnectionString = connectionString,
				Timeout = timeout,
			}, moreThanOne);
		}

		public static IServiceCollection AddMongo<T, TConfig>(this IServiceCollection services) where TConfig : class, IMongoConfig<T>
		{
			return services
				.AddTransient<IMongoConfig<T>, TConfig>()
				.AddTransient<IMongoService<T>, MongoService<T>>();
		}

		public static IServiceCollection AddMongo<T>(this IServiceCollection services, IMongoConfig<T> config)
		{
			return services
				.AddSingleton(config)
				.AddTransient<IMongoService<T>, MongoService<T>>();
		}

		public static IServiceCollection AddMongo<T>(this IServiceCollection services, string connectionString, string databaseName, string collectionName)
		{
			return AddMongo<T>(services, new MongoConfig<T>
			{
				ConnectionString = connectionString,
				DatabaseName = databaseName,
				CollectionName = collectionName
			});
		}
	}
}
