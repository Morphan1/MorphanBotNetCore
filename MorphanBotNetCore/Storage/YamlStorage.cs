﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;

namespace MorphanBotNetCore.Storage
{
    /// <summary>
    /// YAML file-based storage provider.
    /// </summary>
    public class YamlStorage : IStructuredStorage
    {
        /// <summary>
        /// The YAML deserializer.
        /// </summary>
        public IDeserializer Deserializer;

        /// <summary>
        /// The YAML serializer.
        /// </summary>
        public ISerializer Serializer;

        /// <summary>
        /// The file extension this storage supports.
        /// </summary>
        public string FileExtension => "yml";

        /// <summary>
        /// Creates a new YAML storage provider with the specified naming convention.
        /// </summary>
        /// <param name="convention">The naming convention to use.</param>
        public YamlStorage(INamingConvention convention)
        {
            Deserializer = new DeserializerBuilder()
                .WithNamingConvention(convention)
                .Build();
            Serializer = new SerializerBuilder()
                .WithNamingConvention(convention)
                .Build();
        }

        /// <summary>
        /// Reads a data structure from the specified stream.
        /// <para>This will not close the stream.</para>
        /// </summary>
        /// <typeparam name="T">The data structure type.</typeparam>
        /// <returns>A data structure, filled if possible.</returns>
        public T Load<T>(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream, Utilities.DefaultEncoding))
            {
                return Deserializer.Deserialize<T>(reader);
            }
        }

        /// <summary>
        /// Writes a data structure to the specified stream.
        /// <para>This will not close the stream.</para>
        /// </summary>
        /// <typeparam name="T">The data structure type.</typeparam>
        public void Write<T>(Stream stream, T data)
        {
            using (StreamWriter writer = new StreamWriter(stream, Utilities.DefaultEncoding))
            {
                Serializer.Serialize(writer, data, typeof(T));
            }
        }
    }
}
