using System;

namespace cohort.Models
{
    public class Token
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Guid Value { get; set; }
        public Token()
        {
            Value = Guid.NewGuid();
        }
    }
}