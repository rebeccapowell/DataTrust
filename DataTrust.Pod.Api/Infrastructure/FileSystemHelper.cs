using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataTrust.Pod.Api.Infrastructure
{
    public static class FileSystemHelper
    {
        public static DirectoryInfo GetDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }
    }
}
