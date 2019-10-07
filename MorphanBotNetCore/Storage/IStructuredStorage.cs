using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MorphanBotNetCore.Storage
{
    /// <summary>
    /// Represents a type of data storage that supports structured data.
    /// </summary>
    public interface IStructuredStorage
    {
        /// <summary>
        /// The file extension this storage supports.
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Reads a data structure from the specified stream.
        /// <para>This will not close the stream.</para>
        /// </summary>
        /// <typeparam name="T">The data structure type.</typeparam>
        /// <returns>A data structure, filled if possible.</returns>
        T Load<T>(Stream stream);

        /// <summary>
        /// Writes a data structure to the specified stream.
        /// <para>This will not close the stream.</para>
        /// </summary>
        /// <typeparam name="T">The data structure type.</typeparam>
        void Write<T>(Stream stream, T data);
    }
}
