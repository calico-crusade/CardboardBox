using System.Xml.Linq;

namespace CardboardBox
{
	public static class XmlExtensions
	{
		/// <summary>
		/// Adds the given attribute to the target element
		/// </summary>
		/// <param name="el">The target element</param>
		/// <param name="name">The name of the attribute</param>
		/// <param name="value">The value of the attribute</param>
		/// <returns>The target element (for fluent chaining)</returns>
		public static XElement AddAttribute(this XElement el, XName name, object? value)
		{
			el.SetAttributeValue(name, value);
			return el;
		}

		/// <summary>
		/// Adds the given attributes to the target element
		/// </summary>
		/// <param name="el">The target element</param>
		/// <param name="attributes">The attributes to add to the element</param>
		/// <returns>The target element (for fluent chaining)</returns>
		public static XElement AddAttributes(this XElement el, params (XName Attribute, object? Value)[] attributes)
		{
			foreach (var (atr, val) in attributes)
				el.AddAttribute(atr, val);
			return el;
		}

		/// <summary>
		/// Adds the given elements as children to the target element
		/// </summary>
		/// <param name="parent">The target element</param>
		/// <param name="children">The children to add to the element</param>
		/// <returns>The target element (for fluent chaining)</returns>
		public static XElement AddElements(this XElement parent, params object?[] children)
		{
			parent.Add(children);
			return parent;
		}

		/// <summary>
		/// Adds the element as a child to the target element
		/// </summary>
		/// <param name="parent">The target element</param>
		/// <param name="tag">The child elements tag</param>
		/// <param name="value">The child elements value</param>
		/// <param name="attributes">The child elements attributes</param>
		/// <returns>The target element (for fluent chaining)</returns>
		public static XElement AddElement(this XElement parent, XName tag, object? value, params (XName Attribute, object? Value)[] attributes)
		{
			var el = new XElement(tag)
				.AddAttributes(attributes);

			if (value != null)
				el.AddElements(value);

			parent.AddElements(el);
			return parent;
		}

		/// <summary>
		/// Adds the element as a child to the target element, if the value is null, it skips adding the element all together
		/// </summary>
		/// <param name="parent">The target element</param>
		/// <param name="tag">The child elements tag</param>
		/// <param name="value">The child elements value</param>
		/// <param name="attributes">The child elements attributes</param>
		/// <returns>The target element (for fluent chaining)</returns>
		public static XElement AddOptElement(this XElement parent, XName tag, object? value, params (XName Attribute, object? Value)[] attributes)
		{
			if (value == null) return parent;

			return parent.AddElement(tag, value, attributes);
		}
	}
}
