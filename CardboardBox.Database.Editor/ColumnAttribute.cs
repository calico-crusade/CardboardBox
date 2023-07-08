using System;

namespace CardboardBox.Database.Editor
{
	/// <summary>
	/// Represents a column
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class ColumnAttribute : Attribute
	{
		/// <summary>
		/// The name of the column. This should include any escape characters.
		/// This will default to the class name if not specified.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// The is the name you want it to appear as on the UI.
		/// This will default to the class name if not specified.
		/// </summary>
		public string? DisplayName { get; set; }
		
		/// <summary>
		/// The data type of the column.
		/// This will default to the C# type if not specified.
		/// </summary>
		public DataType? Type { get; set; }
	}
}
