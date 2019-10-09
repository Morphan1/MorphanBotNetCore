using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MorphanBotNetCore.Storage
{
    public static class StorageExtensions
    {
        public static T Load<T>(this IStructuredStorage storage, string fileName)
        {
            fileName += "." + storage.FileExtension;
            if (!File.Exists(fileName))
            {
                return default;
            }
            using (FileStream stream = File.OpenRead(fileName))
            {
                return storage.Load<T>(stream);
            }
        }
        public static void Write<T>(this IStructuredStorage storage, string fileName, T data)
        {
            fileName += "." + storage.FileExtension;
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (FileStream stream = File.OpenWrite(fileName))
            {
                storage.Write(stream, data);
            }
        }
    }
}
