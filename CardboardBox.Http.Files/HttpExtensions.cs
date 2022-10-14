namespace CardboardBox.Http
{
	public static class HttpExtensions
	{
		public const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";

		/// <summary>
		/// Requests a network resource from the given url
		/// </summary>
		/// <param name="api">The Api Service object to attach to</param>
		/// <param name="url">The URL resource to fetch</param>
		/// <param name="config">Any optional configuration parameters</param>
		/// <param name="userAgent">The user-agent to use for the request (defaults to: <see cref="USER_AGENT"/></param>
		/// <returns>The stream, file length, filename, and mimetype from the network request</returns>
		/// <exception cref="NullReferenceException">Thrown if the returned request is null upon attempting the read</exception>
		public static async Task<(Stream data, long length, string filename, string type)> GetData(
			this IApiService api, string url, 
			Action<HttpRequestMessage>? config = null, string userAgent = USER_AGENT)
		{
			var req = await api.Create(url)
				.Accept("*/*")
				.With(c =>
				{
					c.Headers.Add("user-agent", userAgent);
					config?.Invoke(c);
				})
				.Result();
			if (req == null)
				throw new NullReferenceException($"Request returned null for: {url}");

			req.EnsureSuccessStatusCode();

			var headers = req.Content.Headers;
			var path = headers?.ContentDisposition?.FileName ?? headers?.ContentDisposition?.Parameters?.FirstOrDefault()?.Value ?? "";
			var type = headers?.ContentType?.ToString() ?? "";
			var length = headers?.ContentLength ?? 0;

			return (await req.Content.ReadAsStreamAsync(), length, path, type);
		}
	}
}
