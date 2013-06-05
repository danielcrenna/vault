using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using copper.Extensions;

namespace copper
{
    /// <summary>
    /// A consumer that handles a stream and saves it to disk
    /// </summary>
    public class FileConsumer : AsyncConsumer<Stream>
    {
        private readonly string _baseDirectory;
        private readonly string _extension;

        public FileConsumer(string baseDirectory) : this(baseDirectory, ".dat")
        {
            
        }

        public FileConsumer(string baseDirectory, string extension)
        {
            _baseDirectory = baseDirectory;
            _extension = extension.StartsWith(".") ? extension : "." + extension;
        }

        public async void Save(Stream @event)
        {
            var folder = _baseDirectory;
            var path = Path.Combine(folder, string.Concat(Guid.NewGuid(), _extension));
            var file = File.OpenWrite(path);
            await @event.CopyToAsync(file);
        }

        public override bool Handle(Stream @event)
        {
            try
            {
                Save(@event);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A consumer that handles an event and streams it to disk
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FileConsumer<T> : AsyncConsumer<T>
    {
        private readonly Serializer _serializer;
        private readonly string _extension;

        public string BaseDirectory { get; private set; }

        public FileConsumer() : this(new BinarySerializer(), GetExecutingDirectory(), ".dat")
        {

        }

        public FileConsumer(string baseDirectory) : this(new BinarySerializer(), baseDirectory, ".dat")
        {

        }

        public FileConsumer(string baseDirectory, string extension) : this(new BinarySerializer(), baseDirectory, extension)
        {
            
        }
        
        public FileConsumer(Serializer serializer, string baseDirectory, string extension)
        {
            _serializer = serializer;
            BaseDirectory = baseDirectory;
            _extension = extension.StartsWith(".") ? extension : "." + extension;
        }

        private static string GetExecutingDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public void Save(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var folder = BaseDirectory;
            var path = Path.Combine(folder, string.Concat(Guid.NewGuid(), _extension));
            using(var file = File.OpenWrite(path))
            {
                stream.CopyTo(file);
                file.Flush();    
            }
        }

        public override bool Handle(T @event)
        {
            try
            {
                var stream = _serializer.SerializeToStream(@event);
                Save(stream);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}