namespace NaiveChain.Models
{
    public interface IHashProvider
    {
        byte[] ComputeHashBytes(string any);
        byte[] ComputeHashBytes(object instance);
        string ComputeHashString(object instance);
        string ComputeHashString(string any);
    }
}