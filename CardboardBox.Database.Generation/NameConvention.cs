namespace CardboardBox.Database.Generation
{
	public interface INameConvention
	{
		string? ToCase(string text);
	}

	public class CamelCaseNameConventon : INameConvention
	{
		public string? ToCase(string text) => text.ToSnakeCase();
	}

	public class PascalCaseNameConvention : INameConvention
	{
		public string? ToCase(string text) => text.ToPascalCase();
	}
}
