using System;
using ZeroMQ;

namespace copper.ZeroMQ
{
    /// <summary>
    /// A thin wrapper around ZMQ for publishing a message
    /// </summary>
    public class ZmqPublisher : IDisposable 
    {
        private ZmqContext _context;
        private ZmqSocket _socket;
        private readonly string _endpoint;

        public ZmqPublisher(string endpoint)
        {
            _endpoint = endpoint;
            _context = ZmqContext.Create();
            _socket = _context.CreateSocket(SocketType.PUB);
            _socket.Bind(_endpoint);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
            if (_context == null)
            {
                return;
            }
            _context.Dispose();
            _context = null;
        }

        public SendStatus Send(byte[] data)
        {
            return _socket.Send(data);
        }
    }
}