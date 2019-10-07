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
            using (FileStream stream = File.OpenRead($"{fileName}.{storage.FileExtension}"))
            {
                return storage.Load<T>(stream);
            }
        }
    }
}
