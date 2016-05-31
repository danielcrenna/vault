using System;
using TableDescriptor;

namespace cohort.Models
{
    public class Error
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string User { get; set; }

        [Computed]
        public DateTime CreatedAt { get; set; }
    }
}