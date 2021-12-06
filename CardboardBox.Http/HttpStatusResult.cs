using System.Net;

namespace CardboardBox.Http
{
	/// <summary>
	/// Represents the results of a HTTP request
	/// </summary>
	/// <typeparam name="TSuccess">The type to use for a successful request</typeparam>
	/// <typeparam name="TFailure">The type to use for a failed request</typeparam>
	public class HttpStatusResult<TSuccess, TFailure>
	{
		/// <summary>
		/// Whether or not the request was successful
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// The status code the request finished with
		/// </summary>
		public HttpStatusCode Code { get; set; }

		/// <summary>
		/// The successful result
		/// </summary>
		public TSuccess? Result { get; set; }

		/// <summary>
		/// The failed result
		/// </summary>
		public TFailure? ErrorResult { get; set; }

		/// <summary>
		/// Creates the result from the given successful data
		/// </summary>
		/// <param name="result">The successful data</param>
		/// <param name="code">The status code the request finished with</param>
		/// <returns>The resulting successful data</returns>
		public static HttpStatusResult<TSuccess, TFailure> FromSuccess(TSuccess? result, HttpStatusCode code)
		{
			return new HttpStatusResult<TSuccess, TFailure>
			{
				Code = code,
				Success = true,
				Result = result
			};
		}

		/// <summary>
		/// Creates the result from the given failed data
		/// </summary>
		/// <param name="result">The failed data</param>
		/// <param name="code">The status code the request finished with</param>
		/// <returns>The resulting failed data</returns>
		public static HttpStatusResult<TSuccess, TFailure> FromFailure(TFailure? result, HttpStatusCode code)
		{
			return new HttpStatusResult<TSuccess, TFailure>
			{
				Code = code,
				Success = false,
				ErrorResult = result
			};
		}
	}
}
