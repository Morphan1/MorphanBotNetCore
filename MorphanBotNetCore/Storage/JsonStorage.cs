using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MorphanBotNetCore.Storage
{
    /// <summary>
    /// JSON storage provider.
    /// </summary>
    public class JsonStorage : IStructuredStorage
    {
        /// <summary>
        /// The JSON serializer and deserializer.
        /// </summary>
        public JsonSerializer Serializer;

        /// <summary>
        /// The file extension this storage supports.
        /// </summary>
        public string FileExtension => "json";

        /// <summary>
        /// Creates a new JSON storage provider and optionally configures it.
        /// </summary>
        /// <param name="configure">Configures the serializer/deserializer.</param>
        public JsonStorage(Action<JsonSerializer> configure = null)
        {
            Serializer = new JsonSerializer();
            configure?.Invoke(Serializer);
        }

        /// <summary>
        /// Reads a data structure from the specified stream.
        /// <para>This will not close the stream.</para>
        /// </summary>
        /// <typeparam name="T">The data structure type.</typeparam>
        /// <returns>A data structure, filled if possible.</returns>
        public T Load<T>(Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream, Utilities.DefaultEncoding))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                return Serializer.Deserialize<T>(reader);
            }
        }

        /// <summary>
        /// Writes a data structure to the specified stream.
        /// <para>This will not close the stream.</para>
        /// </summary>
        /// <typeparam name="T">The data structure type.</typeparam>
        public void Write<T>(Stream stream, T data)
        {
            using (StreamWriter sw = new StreamWriter(stream, Utilities.DefaultEncoding))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                Serializer.Serialize(writer, data, typeof(T));
            }
        }
    }
}
