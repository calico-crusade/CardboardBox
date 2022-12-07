using System.Security.Cryptography;
using System.Text;

namespace CardboardBox
{
	public static class CryptoExtensions
	{
		public static Encoding DefaultEncoding = Encoding.UTF8;

		/// <summary>
		/// Generates an MD5 Hash of the given string
		/// </summary>
		/// <param name="data">The string to hash</param>
		/// <param name="encoding">The encoding to use for getting the byte array</param>
		/// <returns>The MD5 hash of the given string</returns>
		public static string MD5Hash(this string data, Encoding? encoding = null)
		{
			using var md5 = MD5.Create();
			return data.Hash(md5, encoding);
		}

		/// <summary>
		/// Generates an MD5 hash of the given byte array
		/// </summary>
		/// <param name="data">The byte array to hash</param>
		/// <returns>The MD5 hash of the given byte array</returns>
		public static string MD5Hash(this byte[] data)
		{
			using var md5 = MD5.Create();
			return data.Hash(md5);
		}

		/// <summary>
		/// Generates a SHA512 hash of the given string
		/// </summary>
		/// <param name="data">The string to hash</param>
		/// <param name="encoding">The encoding to use for getting the byte array</param>
		/// <returns>The SHA512 hash of the given string</returns>
		public static string SHA512Hash(this string data, Encoding? encoding = null)
		{
			using var sha = SHA512.Create();
			return data.Hash(sha, encoding);
		}

		/// <summary>
		/// Generates a SHA512 hash of the given byte array
		/// </summary>
		/// <param name="data">The byte array to hash</param>
		/// <returns>The SHA512 hash of the given byte array</returns>
		public static string SHA512Hash(this byte[] data)
		{
			using var sha = SHA512.Create();
			return data.Hash(sha);
		}

		/// <summary>
		/// Generates a SHA256 hash of the given string
		/// </summary>
		/// <param name="data">The string to hash</param>
		/// <param name="encoding">The encoding to use for getting the byte array</param>
		/// <returns>The SHA256 hash of the given string</returns>
		public static string SHA256Hash(this string data, Encoding? encoding = null)
		{
			using var sha = SHA256.Create();
			return data.Hash(sha, encoding);
		}

		/// <summary>
		/// Generates a SHA256 hash of the given byte array
		/// </summary>
		/// <param name="data">The byte array to hash</param>
		/// <returns>The SHA256 hash of the given byte array</returns>
		public static string SHA256Hash(this byte[] data)
		{
			using var sha = SHA256.Create();
			return data.Hash(sha);
		}

		/// <summary>
		/// Generates a SHA384 hash of the given string
		/// </summary>
		/// <param name="data">The string to hash</param>
		/// <param name="encoding">The encoding to use for getting the byte array</param>
		/// <returns>The SHA384 hash of the given string</returns>
		public static string SHA384Hash(this string data, Encoding? encoding = null)
		{
			using var sha = SHA384.Create();
			return data.Hash(sha, encoding);
		}

		/// <summary>
		/// Generates a SHA384 hash of the given byte array
		/// </summary>
		/// <param name="data">The byte array to hash</param>
		/// <returns>The SHA384 hash of the given byte array</returns>
		public static string SHA384Hash(this byte[] data)
		{
			using var sha = SHA384.Create();
			return data.Hash(sha);
		}

		/// <summary>
		/// Generates a hash from the given string
		/// </summary>
		/// <param name="data">The string to hash</param>
		/// <param name="algorithm">The algorithm to use</param>
		/// <param name="encoding">The encoding to use for getting the byte array</param>
		/// <returns>The hash of the given string</returns>
		public static string Hash(this string data, HashAlgorithm algorithm, Encoding? encoding = null)
		{
			var input = (encoding ?? DefaultEncoding).GetBytes(data);
			return input.Hash(algorithm);
		}

		/// <summary>
		/// Generates a hash from the given byte array
		/// </summary>
		/// <param name="data">The byte array to hash</param>
		/// <param name="algorithm">The algorithm to use</param>
		/// <returns>The hash of the given string</returns>
		public static string Hash(this byte[] data, HashAlgorithm algorithm)
		{
			var output = algorithm.ComputeHash(data);
			return output.ToHexString();
		}
	}
}
