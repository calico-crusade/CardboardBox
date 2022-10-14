using System.Reflection;

namespace CardboardBox
{
	public static class ReflectionExtensions
	{
		/// <summary>
		/// Sets a _private_ Property Value from a given Object. Uses Reflection.
		/// Throws a ArgumentOutOfRangeException if the Property is not found.
		/// </summary>
		/// <param name="obj">Object from where the Property Value is set</param>
		/// <param name="propName">Propertyname as string.</param>
		/// <param name="val">Value to set.</param>
		/// <returns>PropertyValue</returns>
		public static void SetPrivatePropertyValue(this object obj, string propName, object val)
		{
			Type t = obj.GetType();
			if (t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null)
				throw new ArgumentOutOfRangeException("propName", string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
			t.InvokeMember(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, obj, new object[] { val });
		}

		/// <summary>
		/// Set a private Property Value on a given Object. Uses Reflection.
		/// </summary>
		/// <param name="obj">Object from where the Property Value is returned</param>
		/// <param name="propName">Propertyname as string.</param>
		/// <param name="val">the value to set</param>
		/// <exception cref="ArgumentOutOfRangeException">if the Property is not found</exception>
		public static void SetPrivateFieldValue(this object obj, string propName, object val)
		{
			if (obj == null) throw new ArgumentNullException("obj");
			Type? t = obj.GetType();
			FieldInfo? fi = null;
			while (fi == null && t != null)
			{
				fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				t = t.BaseType;
			}
			if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
			fi.SetValue(obj, val);
		}
	}
}
