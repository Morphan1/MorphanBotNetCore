using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Storage
{
    public class FileReferenceAttribute : Attribute
    {
        public bool List;

        public string Folder;
    }

    public class FileNameAttribute : Attribute
    {
    }
}
