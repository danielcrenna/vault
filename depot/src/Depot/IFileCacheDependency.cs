using System;
using System.Collections.Generic;

namespace depot
{
    public interface IFileCacheDependency : ICacheDependency
    {
        IList<string> FilePaths { get; }
        IFileCacheDependency Create(string path);
        IFileCacheDependency Create(string path, DateTime start);
        IFileCacheDependency Create(string[] paths);
    }
}