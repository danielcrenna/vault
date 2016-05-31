using System;
using TableDescriptor;

namespace cohort
{
    public class Log
    {
        public int Id { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        
        public string User { get; set; }
        public string IPAddress { get; set; }
        public string Path { get; set; }

        [Computed]
        public DateTime CreatedAt { get; set; }
    }
}