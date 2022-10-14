using Dapper.FluentMap;
using Dapper.FluentMap.Configuration;
using Dapper.FluentMap.Conventions;

namespace CardboardBox.Database.Generation
{
	public static class MapConfig
	{
		private static readonly List<Action<FluentConventionConfiguration>> _actions = new();

		public static void AddMap(Action<FluentConventionConfiguration> action)
		{
			_actions.Add(action);
		}

		public static void StartMap<TConvention>() where TConvention : Convention, new()
		{
			FluentMapper.Initialize(config =>
			{
				var conv = config
					.AddConvention<TConvention>();

				foreach (var action in _actions)
					action?.Invoke(conv);
			});
		}

		public static void StartCamelCaseMap() => StartMap<CamelCaseMap>();
	}
}
