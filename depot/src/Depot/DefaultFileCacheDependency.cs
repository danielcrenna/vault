using System;
using System.Collections.Generic;
using System.IO;

namespace depot
{
    public class DefaultFileCacheDependency : IFileCacheDependency
    {
        public void Dispose() { }
        public DefaultFileCacheDependency() { FilePaths = new List<string>(); }
        private DefaultFileCacheDependency(string path) : this()
        {
            FilePaths.Add(path);
            Id = Guid.NewGuid().ToString();
            var fi = new FileInfo(path);
            LastModified = fi.LastWriteTime;
        }
        private DefaultFileCacheDependency(string path, DateTime start) : this(path)
        {
            ShouldInvalidate = LastModified > start;
        }
        private DefaultFileCacheDependency(params string[] paths) : this()
        {
            foreach(var path in paths) FilePaths.Add(path);
        }

        public string Id { get; private set; }
        public bool ShouldInvalidate { get; private set; }
        public DateTime LastModified { get; private set; }
        public IList<string> FilePaths { get; private set; }
        public IFileCacheDependency Create(string path)
        {
            return new DefaultFileCacheDependency(path);
        }
        public IFileCacheDependency Create(string path, DateTime start)
        {
            return new DefaultFileCacheDependency(path, start);
        }
        public IFileCacheDependency Create(string[] paths)
        {
            return new DefaultFileCacheDependency(paths);
        }
    }
}