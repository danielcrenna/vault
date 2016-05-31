using System;
using System.Collections.Generic;

namespace linger
{
    [Serializable]
    public class Batch
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public IEnumerable<ScheduledJob> Jobs { get; set; }
        public int Priority { get; set; }

        public Batch()
        {
            Jobs = new List<ScheduledJob>();
        }
    }
}