using System;

namespace CardboardBox.Database.Editor
{
	/// <summary>
	/// Represents a database object
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class TableAttribute : Attribute
	{
		/// <summary>
		/// The name of the database object. This should be a fully qualified name, including catelog and schema (if necessary).
		/// This will default to the class name if not specified.
		/// </summary>
		public string? Name { get; set; }
	}
}
