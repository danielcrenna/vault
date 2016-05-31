using System;
using System.Collections.Generic;
using System.Web.Caching;

namespace depot.AspNet
{
    public class AspNetFileCacheDependency : IFileCacheDependency
    {
        public CacheDependency Internal { get; private set; }

        public AspNetFileCacheDependency() { FilePaths = new List<string>(); }

        private AspNetFileCacheDependency(string filename) : this()
        {
            FilePaths.Add(filename);
            Internal = new CacheDependency(filename);
        }

        private AspNetFileCacheDependency(string filename, DateTime start) : this()
        {
            FilePaths.Add(filename);
            Internal = new CacheDependency(filename, start);
        }

        private AspNetFileCacheDependency(string[] filenames) : this()
        {
            foreach(var filename in filenames) FilePaths.Add(filename);
            Internal = new CacheDependency(filenames);
        }

        public void Dispose()
        {
            Internal.Dispose();
        }
        
        public string Id
        {
            get { return Internal.GetUniqueID(); }
        }

        public bool ShouldInvalidate
        {
            get { return Internal.HasChanged; }
        }

        public DateTime LastModified
        {
            get { return Internal.UtcLastModified; }
        }

        public IList<string> FilePaths { get; private set; }

        public IFileCacheDependency Create(string path)
        {
            return new AspNetFileCacheDependency(path);
        }

        public IFileCacheDependency Create(string path, DateTime start)
        {
            return new AspNetFileCacheDependency(path, start);
        }

        public IFileCacheDependency Create(string[] paths)
        {
            return new AspNetFileCacheDependency(paths);
        }
    }
}