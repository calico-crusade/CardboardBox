using Newtonsoft.Json;
using STJ = System.Text.Json;

namespace CardboardBox.Json
{
	/// <summary>
	/// Exposes common Json serialization and deserialization methods
	/// </summary>
	public interface IJsonService
	{
		/// <summary>
		/// Deserializes the given string from JSON to the given type
		/// </summary>
		/// <typeparam name="T">The type to deserialize to</typeparam>
		/// <param name="data">The JSON string</param>
		/// <returns>The deserialized result</returns>
		T? Deserialize<T>(string data);

		/// <summary>
		/// Deserializes the given stream from JSON to the given type
		/// </summary>
		/// <typeparam name="T">The type to deserialize to</typeparam>
		/// <param name="stream">The stream of JSON data</param>
		/// <returns>A task representing the deserialized result</returns>
		Task<T?> Deserialize<T>(Stream stream);

		/// <summary>
		/// Serializes the given data to JSON
		/// </summary>
		/// <typeparam name="T">The type of data to serialize</typeparam>
		/// <param name="data">The data to serialize</param>
		/// <returns>The serialized JSON</returns>
		string Serialize<T>(T data);

		/// <summary>
		/// Serializes the given data to JSON
		/// </summary>
		/// <typeparam name="T">The type of data to serialize</typeparam>
		/// <param name="data">The data to serialize</param>
		/// <param name="stream">The stream to write the serialized data to</param>
		/// <returns>A task representing the completion of the serialization process</returns>
		Task Serialize<T>(T data, Stream stream);
	}

	/// <summary>
	/// A concrete implementation of <see cref="IJsonService"/> that uses <see cref="JsonConvert"/> from Newtonsoft.Json
	/// </summary>
	public class NewtonsoftJsonService : IJsonService
	{
		/// <summary>
		/// Deserializes the given string from JSON to the given type
		/// </summary>
		/// <typeparam name="T">The type to deserialize to</typeparam>
		/// <param name="data">The JSON string</param>
		/// <returns>The deserialized result</returns>
		public T? Deserialize<T>(string data) => JsonConvert.DeserializeObject<T>(data);

		/// <summary>
		/// Deserializes the given stream from JSON to the given type
		/// </summary>
		/// <typeparam name="T">The type to deserialize to</typeparam>
		/// <param name="stream">The stream of JSON data</param>
		/// <returns>A task representing the deserialized result</returns>
		public Task<T?> Deserialize<T>(Stream stream)
		{
			var ser = new JsonSerializer();
			using var sr = new StreamReader(stream);
			using var jtr = new JsonTextReader(sr);
			
			return Task.FromResult(ser.Deserialize<T>(jtr));
		}

		/// <summary>
		/// Serializes the given data to JSON
		/// </summary>
		/// <typeparam name="T">The type of data to serialize</typeparam>
		/// <param name="data">The data to serialize</param>
		/// <returns>The serialized JSON</returns>
		public string Serialize<T>(T data) => JsonConvert.SerializeObject(data);

		/// <summary>
		/// Serializes the given data to JSON
		/// </summary>
		/// <typeparam name="T">The type of data to serialize</typeparam>
		/// <param name="data">The data to serialize</param>
		/// <param name="stream">The stream to write the serialized data to</param>
		/// <returns>A task representing the completion of the serialization process</returns>
		public async Task Serialize<T>(T data, Stream stream)
		{
			var ser = new JsonSerializer();
			using var sw = new StreamWriter(stream);
			using var jtw = new JsonTextWriter(sw);

			ser.Serialize(jtw, data);
			await jtw.FlushAsync();
		}
	}

	/// <summary>
	/// A concrete implementation of <see cref="IJsonService"/> that uses <see cref="STJ.JsonSerializer"/> from System.Text.Json
	/// </summary>
	public class SystemTextJsonService : IJsonService
	{
		private readonly STJ.JsonSerializerOptions _settings;

		public SystemTextJsonService(STJ.JsonSerializerOptions settings)
		{
			_settings = settings;
		}

		/// <summary>
		/// Deserializes the given string from JSON to the given type
		/// </summary>
		/// <typeparam name="T">The type to deserialize to</typeparam>
		/// <param name="data">The JSON string</param>
		/// <returns>The deserialized result</returns>
		public T? Deserialize<T>(string data) => STJ.JsonSerializer.Deserialize<T>(data, _settings);

		/// <summary>
		/// Deserializes the given stream from JSON to the given type
		/// </summary>
		/// <typeparam name="T">The type to deserialize to</typeparam>
		/// <param name="stream">The stream of JSON data</param>
		/// <returns>A task representing the deserialized result</returns>
		public async Task<T?> Deserialize<T>(Stream stream) => await STJ.JsonSerializer.DeserializeAsync<T>(stream, _settings);

		/// <summary>
		/// Serializes the given data to JSON
		/// </summary>
		/// <typeparam name="T">The type of data to serialize</typeparam>
		/// <param name="data">The data to serialize</param>
		/// <returns>The serialized JSON</returns>
		public string Serialize<T>(T data) => STJ.JsonSerializer.Serialize(data, _settings);

		/// <summary>
		/// Serializes the given data to JSON
		/// </summary>
		/// <typeparam name="T">The type of data to serialize</typeparam>
		/// <param name="data">The data to serialize</param>
		/// <param name="stream">The stream to write the serialized data to</param>
		/// <returns>A task representing the completion of the serialization process</returns>
		public Task Serialize<T>(T data, Stream stream) => STJ.JsonSerializer.SerializeAsync(stream, data, _settings);
	}
}