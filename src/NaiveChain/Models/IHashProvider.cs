namespace NaiveChain.Models
{
    public interface IHashProvider
    {
        byte[] ComputeHashBytes(string any);
        byte[] ComputeHashBytes(object instance);
        string ComputeHash(object instance);
        string ComputeHash(string any);
    }
}