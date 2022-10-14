using HtmlAgilityPack;

namespace CardboardBox.Http
{
	public static class HttpExtensions
	{
		public const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";

		/// <summary>
		/// Fetches the <see cref="HtmlDocument"/> from the given network resource
		/// </summary>
		/// <param name="api">The <see cref="IApiService"/> to attach to</param>
		/// <param name="url">The network resource to request from</param>
		/// <param name="config">Any optional configurations</param>
		/// <param name="userAgent">The user-agent to use for the request. Defaults to: <see cref="USER_AGENT"/></param>
		/// <param name="ensureResponse">Whether or not to check for 200 response codes</param>
		/// <returns>The HTML Document</returns>
		public static Task<HtmlDocument> GetHtml(this IApiService api, string url, 
			Action<HttpRequestMessage>? config = null, 
			string userAgent = USER_AGENT,
			bool ensureResponse = true)
		{
			return api.Create(url)
				.Accept("text/html")
				.With(c =>
				{
					c.Headers.Add("user-agent", userAgent);
					config?.Invoke(c);
				})
				.HtmlResult(ensureResponse);
		}

		/// <summary>
		/// Fetches the <see cref="HtmlDocument"/> from the given network resource
		/// </summary>
		/// <param name="builder">The <see cref="IHttpBuilder"/> to attach to</param>
		/// <param name="ensureResponse">Whether or not to check for 200 response codes</param>
		/// <returns>The HTML Document</returns>
		/// <exception cref="NullReferenceException">Thrown if the request returns back a null object</exception>
		public static async Task<HtmlDocument> HtmlResult(this IHttpBuilder builder, bool ensureResponse = true)
		{
			using var resp = await builder.Result();
			if (resp == null) throw new NullReferenceException($"Request returned null!");

			if (ensureResponse) resp.EnsureSuccessStatusCode();

			using var io = await resp.Content.ReadAsStreamAsync();
			var doc = new HtmlDocument();
			doc.Load(io);
			return doc;
		}
	}
}