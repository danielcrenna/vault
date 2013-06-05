using System;
using System.Collections.Generic;

namespace protobuf.Poco.Tests
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public byte[] Image { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; }
    }
}