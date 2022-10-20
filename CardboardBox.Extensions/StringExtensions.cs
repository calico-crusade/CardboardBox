using System.Security.Cryptography;
using System.Text;

namespace CardboardBox
{
	public static class StringExtensions
	{
		/// <summary>
		/// Generates an MD5 Hash of the given string
		/// </summary>
		/// <param name="data">The string to hash</param>
		/// <returns>The MD5 hash of the given string</returns>
		public static string MD5Hash(this string data)
		{
			using var md5 = MD5.Create();
			var input = Encoding.UTF8.GetBytes(data);
			var output = md5.ComputeHash(input);
			return output.ToHexString();
		}

		/// <summary>
		/// Converts the given byte array into a HEX string (polyfil for Convert.ToHexString in .net 5+)
		/// </summary>
		/// <param name="data">The byte array data to convert</param>
		/// <returns>The byte array represented as a hex string</returns>
		public static string ToHexString(this byte[] data)
		{
			var bob = new StringBuilder();
			foreach (byte b in data)
				bob.AppendFormat("{0:x2}", b);
			return bob.ToString();
		}

		/// <summary>
		/// Deteremines whether or not the given string is comprised only of white space characters.
		/// This includes the zero-width character
		/// </summary>
		/// <param name="value">The string to check</param>
		/// <returns>Whether or not the given string is only comprised of white-space characters.</returns>
		public static bool IsWhiteSpace(this string? value)
		{
			var isWs = (char c) => char.IsWhiteSpace(c) || c == '\u00A0';
			if (value == null || value.Length == 0) return true;

			for (var i = 0; i < value.Length; i++)
				if (!isWs(value[i])) return false;

			return true;
		}

		/// <summary>
		/// Converts the given string to snake_case (Snake Case -> snake_case)
		/// </summary>
		/// <param name="text">The text to convert</param>
		/// <returns>The snake_case version of the given string</returns>
		public static string? ToSnakeCase(this string? text)
		{
			if (string.IsNullOrEmpty(text)) return text;
			if (text.Length < 2) return text;

			var sb = new StringBuilder();
			sb.Append(char.ToLowerInvariant(text[0]));
			for (int i = 1; i < text.Length; ++i)
			{
				char c = text[i];
				if (!char.IsUpper(c))
				{
					sb.Append(c);
					continue;
				}

				sb.Append('_');
				sb.Append(char.ToLowerInvariant(c));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Converts the given string to pascalCase (PascalCase -> pascalCase)
		/// </summary>
		/// <param name="text">The text to convert</param>
		/// <returns>The pascalCase version of the give string</returns>
		public static string? ToPascalCase(this string? text)
		{
			if (string.IsNullOrEmpty(text)) return text;

			var chars = text.ToCharArray();
			chars[0] = char.ToLowerInvariant(chars[0]);
			return new string(chars);
		}

		/// <summary>
		/// Removes all characters that are invalid for paths and file names.
		/// These characters are sourced from: <see cref="Path.GetInvalidFileNameChars"/> and <see cref="Path.GetInvalidPathChars"/>
		/// </summary>
		/// <param name="text">The string to sanitize</param>
		/// <returns>The sanitized string</returns>
		public static string PurgePathChars(this string text)
		{
			var chars = Path.GetInvalidFileNameChars()
				.Union(Path.GetInvalidPathChars())
				.ToArray();

			foreach (var c in chars)
				text = text.Replace(c.ToString(), "");

			return text;
		}

		/// <summary>
		/// Splits the given string by the given max length
		/// </summary>
		/// <param name="content">The string to split</param>
		/// <param name="maxLength">The max length for each string segment</param>
		/// <returns>The split chunks of the string</returns>
		public static IEnumerable<string> Split(this string content, int maxLength = 2000)
		{
			if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
			{
				yield return content;
				yield break;
			}

			using var r = new StringReader(content);
			string current = "";
			while (true)
			{
				var line = r.ReadLine();
				if (line == null)
				{
					if (!string.IsNullOrEmpty(current))
						yield return current;
					yield break;
				}

				var combinedLength = (current + Environment.NewLine + line).Length;
				if (combinedLength > maxLength)
				{
					yield return current;
					current = line;
					continue;
				}

				current += Environment.NewLine + line;
			}
		}
	}
}
