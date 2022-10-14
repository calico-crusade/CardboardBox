using System.Text.Json.Serialization;

namespace CardboardBox.OpenApi
{
	public class SwaggerJson
	{
		[JsonPropertyName("openapi")]
		public string Version { get; set; } = "";

		[JsonPropertyName("info")]
		public InfoData Info { get; set; } = new();

		[JsonPropertyName("paths")]
		public Dictionary<string, PathData> Paths { get; set; } = new();

		
		public class InfoData
		{
			[JsonPropertyName("title")]
			public string Title { get; set; } = "";

			[JsonPropertyName("description")]
			public string? Description { get; set; }

			[JsonPropertyName("version")]
			public string Version { get; set; } = "";

			[JsonPropertyName("termsOfService")]
			public string? TermsOfService { get; set; }

			[JsonPropertyName("contact")]
			public ContactData? Contact { get; set; }

			[JsonPropertyName("license")]
			public LicenseData? License { get; set; }

		}

		public class ContactData : LicenseData
		{
			[JsonPropertyName("email")]
			public string? Email { get; set; }
		}

		public class LicenseData
		{
			[JsonPropertyName("name")]
			public string Name { get; set; } = "";

			[JsonPropertyName("url")]
			public string? Url { get; set; }
		}

		public class PathData
		{
			[JsonPropertyName("summary")]
			public string? Summary { get; set; }

			[JsonPropertyName("description")]
			public string? Description { get; set; }

			[JsonPropertyName("get")]
			public OperationData? Get { get; set; }

			[JsonPropertyName("put")]
			public OperationData? Put { get; set; }

			[JsonPropertyName("post")]
			public OperationData? Post { get; set; }

			[JsonPropertyName("delete")]
			public OperationData? Delete { get; set; }

			[JsonPropertyName("options")]
			public OperationData? Options { get; set; }

			[JsonPropertyName("head")]
			public OperationData? Head { get; set; }

			[JsonPropertyName("patch")]
			public OperationData? Patch { get; set; }

			[JsonPropertyName("trace")]
			public OperationData? Trace { get; set; }
		}

		public class OperationData
		{
			[JsonPropertyName("tags")]
			public string[]? Tags { get; set; }

			[JsonPropertyName("summary")]
			public string? Summary { get; set; }

			[JsonPropertyName("description")]
			public string? Description { get; set; }

			[JsonPropertyName("operationId")]
			public string? OperationId { get; set; }

			[JsonPropertyName("responses")]
			public Dictionary<string, ResponseData> Responses { get; set; } = new();
		}

		public class ResponseData
		{
			[JsonPropertyName("description")]
			public string Description { get; set; } = "";
		}
	}
}