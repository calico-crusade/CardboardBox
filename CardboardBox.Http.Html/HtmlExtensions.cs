using HtmlAgilityPack;
using System.Web;

namespace CardboardBox.Http
{
	public static class HtmlExtensions
	{
		/// <summary>
		/// Parses the given HTML and returns a <see cref="HtmlDocument"/> that represents the parsed data
		/// </summary>
		/// <param name="html">The string containing the HTML</param>
		/// <returns>The parsed HTML document</returns>
		public static HtmlDocument ParseHtml(this string html)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			return doc;
		}

		/// <summary>
		/// Creates a copy of the given <see cref="HtmlNode"/> as a new document
		/// </summary>
		/// <param name="node">The node to copy</param>
		/// <returns>The copied node</returns>
		public static HtmlNode Copy(this HtmlNode node)
		{
			return node.InnerHtml.ParseHtml().DocumentNode;
		}

		/// <summary>
		/// Decodes the given string. Utilizies <see cref="HttpUtility.HtmlDecode(string?)"/>
		/// </summary>
		/// <param name="text">The text to decode</param>
		/// <returns>The decoded HTML string</returns>
		public static string HTMLDecode(this string text)
		{
			return HttpUtility.HtmlDecode(text).Trim('\n');
		}

		/// <summary>
		/// Gets the scheme, domain and optional port from the given URL
		/// </summary>
		/// <param name="url">The URL to parse the domain from</param>
		/// <returns>The parsed domain</returns>
		/// <exception cref="UriFormatException">Thrown if the URL cannot be parsed to a <see cref="Uri"/></exception>
		public static string GetRootUrl(this string url)
		{
			if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
				throw new UriFormatException(url);

			return uri.GetRootUrl();
		}

		/// <summary>
		/// Gets the scheme, domain, and optional port from the given URL
		/// </summary>
		/// <param name="uri">The URL to parse the domain from</param>
		/// <returns>The parsed domain</returns>
		public static string GetRootUrl(this Uri uri)
		{
			var port = uri.IsDefaultPort ? "" : ":" + uri.Port;
			return $"{uri.Scheme}://{uri.Host}{port}";
		}

		/// <summary>
		/// Gets the inner text string from the given document and xpath
		/// </summary>
		/// <param name="doc">The document to get the inner text from</param>
		/// <param name="xpath">The xpath that targets the node</param>
		/// <param name="shouldDecode">Whether or not to decode any HTML objects</param>
		/// <returns>The inner text of the target element or null if it was not found</returns>
		public static string? InnerText(this HtmlDocument doc, string xpath, bool shouldDecode = true)
		{
			return doc.DocumentNode.InnerText(xpath, shouldDecode);
		}

		/// <summary>
		/// Gets the inner HTML string from the given document and xpath
		/// </summary>
		/// <param name="doc">The document to get the inner HTML from</param>
		/// <param name="xpath">The xpath that targets the node</param>
		/// <param name="shouldDecode">Whether or not to decode any HTML objects</param>
		/// <returns>The inner HTML of the target element or null if it was not found</returns>
		public static string? InnerHtml(this HtmlDocument doc, string xpath, bool shouldDecode = true)
		{
			return doc.DocumentNode.InnerHtml(xpath, shouldDecode);
		}

		/// <summary>
		/// Gets the attribute value from the given document and xpath
		/// </summary>
		/// <param name="doc">The document to get the attribute value from</param>
		/// <param name="xpath">The xpath that targets the node</param>
		/// <param name="attr">The target attribute</param>
		/// <param name="shouldDecode">Whether or not to decode any HTML objects</param>
		/// <returns>The attribute value or null if it was not found</returns>
		public static string? Attribute(this HtmlDocument doc, string xpath, string attr, bool shouldDecode = true)
		{
			return doc.DocumentNode.Attribute(xpath, attr, shouldDecode);
		}

		/// <summary>
		/// Gets the inner text string from the given document and xpath
		/// </summary>
		/// <param name="doc">The document to get the inner text from</param>
		/// <param name="xpath">The xpath that targets the node</param>
		/// <param name="shouldDecode">Whether or not to decode any HTML objects</param>
		/// <returns>The inner text of the target element or null if it was not found</returns>
		public static string? InnerText(this HtmlNode doc, string xpath, bool shouldDecode = true)
		{

			return doc.SelectSingleNode(xpath)?.InnerText?.Decode(shouldDecode);
		}

		/// <summary>
		/// Gets the inner HTML string from the given document and xpath
		/// </summary>
		/// <param name="doc">The document to get the inner HTML from</param>
		/// <param name="xpath">The xpath that targets the node</param>
		/// <param name="shouldDecode">Whether or not to decode any HTML objects</param>
		/// <returns>The inner HTML of the target element or null if it was not found</returns>
		public static string? InnerHtml(this HtmlNode doc, string xpath, bool shouldDecode = true)
		{
			return doc.SelectSingleNode(xpath)?.InnerHtml?.Decode(shouldDecode);
		}

		/// <summary>
		/// Gets the attribute value from the given document and xpath
		/// </summary>
		/// <param name="doc">The document to get the attribute value from</param>
		/// <param name="xpath">The xpath that targets the node</param>
		/// <param name="attr">The target attribute</param>
		/// <param name="shouldDecode">Whether or not to decode any HTML objects</param>
		/// <returns>The attribute value or null if it was not found</returns>
		public static string? Attribute(this HtmlNode doc, string xpath, string attr, bool shouldDecode = true)
		{
			return doc.SelectSingleNode(xpath)?.GetAttributeValue(attr, "")?.Decode(shouldDecode);
		}

		private static string? Decode(this string? input, bool shouldDecode)
		{
			if (input == null || !shouldDecode) return input;

			return input.HTMLDecode();
		}
	}
}
