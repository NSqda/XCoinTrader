using System;
using System.IO;

namespace qda
{
    namespace IO
    {
        public class IO
        {
            private string path;
            private FileStream file;

            public IO (string path, FileMode fileMode)
            {
                file = new FileStream(path, FileMode.Append);
            }
        }
    }
}
