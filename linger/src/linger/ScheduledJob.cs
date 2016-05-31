using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace linger
{
    [Serializable]
    public class ScheduledJob : IEquatable<ScheduledJob>
    {
        public int Id { get; set; }
        public int Priority { get; set; }
        public int Attempts { get; set; }

        [IgnoreDataMember]
        public byte[] Handler { get; set; }
        public string LastError { get; set; }
        public DateTime? RunAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime? SucceededAt { get; set; }
        public DateTime? LockedAt { get; set; }
        public string LockedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public RepeatInfo RepeatInfo { get; internal set; }
        
        public bool Equals(ScheduledJob other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Priority == other.Priority && Attempts == other.Attempts &&
                   string.Equals(LastError, other.LastError) &&
                   RunAt.Equals(other.RunAt) && SucceededAt.Equals(other.SucceededAt) && FailedAt.Equals(other.FailedAt) &&
                   LockedAt.Equals(other.LockedAt) && string.Equals(LockedBy, other.LockedBy) &&
                   CreatedAt.Equals(other.CreatedAt) && UpdatedAt.Equals(other.UpdatedAt);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ScheduledJob) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode*397) ^ Priority;
                hashCode = (hashCode*397) ^ Attempts;
                hashCode = (hashCode*397) ^ (LastError != null ? LastError.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ RunAt.GetHashCode();
                hashCode = (hashCode*397) ^ SucceededAt.GetHashCode();
                hashCode = (hashCode*397) ^ FailedAt.GetHashCode();
                hashCode = (hashCode*397) ^ LockedAt.GetHashCode();
                hashCode = (hashCode*397) ^ (LockedBy != null ? LockedBy.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ CreatedAt.GetHashCode();
                hashCode = (hashCode*397) ^ UpdatedAt.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ScheduledJob left, ScheduledJob right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScheduledJob left, ScheduledJob right)
        {
            return !Equals(left, right);
        }

        private sealed class ScheduledJobEqualityComparer : IEqualityComparer<ScheduledJob>
        {
            public bool Equals(ScheduledJob x, ScheduledJob y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id && x.Priority == y.Priority && x.Attempts == y.Attempts &&
                       string.Equals(x.LastError, y.LastError) && x.RunAt.Equals(y.RunAt) &&
                       x.FailedAt.Equals(y.FailedAt) && x.SucceededAt.Equals(y.SucceededAt) &&
                       x.LockedAt.Equals(y.LockedAt) && string.Equals(x.LockedBy, y.LockedBy) &&
                       x.CreatedAt.Equals(y.CreatedAt) && x.UpdatedAt.Equals(y.UpdatedAt);
            }

            public int GetHashCode(ScheduledJob obj)
            {
                unchecked
                {
                    var hashCode = obj.Id;
                    hashCode = (hashCode*397) ^ obj.Priority;
                    hashCode = (hashCode*397) ^ obj.Attempts;
                    hashCode = (hashCode*397) ^ (obj.LastError != null ? obj.LastError.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ obj.RunAt.GetHashCode();
                    hashCode = (hashCode*397) ^ obj.FailedAt.GetHashCode();
                    hashCode = (hashCode*397) ^ obj.SucceededAt.GetHashCode();
                    hashCode = (hashCode*397) ^ obj.LockedAt.GetHashCode();
                    hashCode = (hashCode*397) ^ (obj.LockedBy != null ? obj.LockedBy.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ obj.CreatedAt.GetHashCode();
                    hashCode = (hashCode*397) ^ obj.UpdatedAt.GetHashCode();
                    return hashCode;
                }
            }
        }

        private static readonly IEqualityComparer<ScheduledJob> ScheduledJobComparerInstance = new ScheduledJobEqualityComparer();

        public static IEqualityComparer<ScheduledJob> ScheduledJobComparer
        {
            get { return ScheduledJobComparerInstance; }
        }
    }
}