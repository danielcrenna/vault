using System;
using ZeroMQ;

namespace copper.ZeroMQ
{
    /// <summary>
    /// A thin wrapper around ZMQ for receiving a message
    /// <remarks>
    /// </remarks>
    /// </summary>
    public class ZmqSubscriber : IDisposable
    {
        private ZmqContext _context;
        private ZmqSocket _socket;
        private readonly string _endpoint;

        public int BufferSize { get; set; }
        
        public ZmqSubscriber(string endpoint)
        {
            _endpoint = endpoint;
            _context = ZmqContext.Create();

            BufferSize = 1024;
            ConnectSocket();
        }

        public void ConnectSocket()
        {
            if (_socket != null)
            {
                return;
            }
            _socket = _context.CreateSocket(SocketType.SUB);
            _socket.Subscribe(new byte[] {});
            _socket.Connect(_endpoint);
        }

        public byte[] Receive()
        {
            int received;
            var message = _socket.Receive(null, out received);
            return message;
        }

        protected virtual void Dispose(bool disposing)
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}