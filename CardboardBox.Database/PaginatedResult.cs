using System.Text.Json.Serialization;

namespace CardboardBox.Database
{
	public class PaginatedResult<T>
	{
		[JsonPropertyName("pages")]
		public long Pages { get; set; }

		[JsonPropertyName("count")]
		public long Count { get; set; }

		[JsonPropertyName("results")]
		public T[] Results { get; set; } = Array.Empty<T>();

		public PaginatedResult() { }

		public PaginatedResult(int pages, int count, T[] results)
		{
			Pages = pages;
			Count = count;
			Results = results;
		}

		public PaginatedResult(long pages, long count, T[] results)
		{
			Pages = pages;
			Count = count;
			Results = results;
		}

		public void Deconstruct(out long pages, out long count, out T[] results)
		{
			pages = Pages;
			count = Count;
			results = Results;
		}
	}
}
