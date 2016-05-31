namespace TableDescriptor.Tests.Models
{
    public class HasTransient
    {
        public int Id { get; set; }
        public string Email { get; set; }

        [Transient]
        public string EmailTrimmed
        {
            get { return Email.Trim(); }
        }
    }

    public class MultipleKeysButIdentityForced
    {
        [Identity]
        public int ThisId { get; set; }
        public int ThatId { get; set; }
    }
}