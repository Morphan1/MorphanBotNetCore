using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Storage
{
    public interface IFileReferenceable
    {
        string GetFilePath();
    }
}
