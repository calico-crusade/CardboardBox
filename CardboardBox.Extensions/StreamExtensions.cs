using System.Text;

namespace CardboardBox
{
	public static class StreamExtensions
	{
		/// <summary>
		/// Converts the given string to a stream
		/// </summary>
		/// <param name="content">The content to stream</param>
		/// <param name="encoding">The optional encoding to use (defaults to <see cref="Encoding.UTF8"/>)</param>
		/// <returns>The contents in stream form</returns>
		public static Stream ToStream(this string content, Encoding? encoding = null)
		{
			var bytes = (encoding ?? Encoding.UTF8).GetBytes(content);
			return ToStream(bytes);
		}

		/// <summary>
		/// Converts the given byte array to a stream
		/// </summary>
		/// <param name="content">The byte array to stream</param>
		/// <returns>The contents in stream form</returns>
		public static Stream ToStream(this byte[] content)
		{
			return new MemoryStream(content);
		}
	}
}
