using System.Reflection;

namespace CardboardBox.Discord.Components
{
	public interface IComponentHandlerService
	{
		/// <summary>
		/// Registers all of the methods that have <see cref="ButtonAttribute"/> or <see cref="SelectMenuAttribute"/> attributes in the given type
		/// </summary>
		/// <param name="type">The type to get the methods from</param>
		/// <exception cref="ArgumentException">Thrown if the type doesn't implement <see cref="ComponentHandler"/></exception>
		void RegisterHandlers(Type type);

		/// <summary>
		/// Generates a unique Id for the given <see cref="MethodInfo"/> that can be used to look up this component
		/// </summary>
		/// <param name="method">The method to generate the Id for</param>
		/// <returns>The unique Id</returns>
		string IdFromMethod(MethodInfo method);

		/// <summary>
		/// Checks if a handler exists for the given Id
		/// </summary>
		/// <param name="id">The Id to check for</param>
		/// <param name="method">The method that was found (null otherwise)</param>
		/// <param name="type">The type that was found (null otherwise)</param>
		/// <returns>Whether or not a handler for the given Id was found</returns>
		bool GetHandler(string id, out MethodInfo? method, out Type? type);
	}

	public class ComponentHandlerService : IComponentHandlerService
	{
		private readonly Dictionary<string, (MethodInfo, Type)> _handlers = new();

		/// <summary>
		/// Checks if a handler exists for the given Id
		/// </summary>
		/// <param name="id">The Id to check for</param>
		/// <param name="method">The method that was found (null otherwise)</param>
		/// <param name="type">The type that was found (null otherwise)</param>
		/// <returns>Whether or not a handler for the given Id was found</returns>
		public bool GetHandler(string id, out MethodInfo? method, out Type? type)
		{
			method = null;
			type = null;

			if (!_handlers.ContainsKey(id))
				return false;

			var (m, t) = _handlers[id];
			method = m;
			type = t;
			return true;
		}

		/// <summary>
		/// Registers all of the methods that have <see cref="ButtonAttribute"/> or <see cref="SelectMenuAttribute"/> attributes in the given type
		/// </summary>
		/// <param name="type">The type to get the methods from</param>
		/// <exception cref="ArgumentException">Thrown if the type doesn't implement <see cref="ComponentHandler"/></exception>
		public void RegisterHandlers(Type type)
		{
			if (!typeof(ComponentHandler).IsAssignableFrom(type))
				throw new ArgumentException($"Type does not implement `{nameof(ComponentHandler)}`", type.FullName);

			var methods = type.GetMethods();
			foreach (var method in methods)
			{
				var compAtr = method.GetCustomAttribute<ComponentAttribute>();
				if (compAtr == null) continue;

				var id = IdFromMethod(method);

				if (!_handlers.ContainsKey(id))
					_handlers.Add(id, (method, type));
			}
		}

		/// <summary>
		/// Generates a unique Id for the given <see cref="MethodInfo"/> that can be used to look up this component
		/// </summary>
		/// <param name="method">The method to generate the Id for</param>
		/// <returns>The unique Id</returns>
		public string IdFromMethod(MethodInfo method)
		{
			return $"{method.DeclaringType?.FullName ?? ("NO_DECLARE_TYPE")}.{method.Name}";
		}
	}
}
