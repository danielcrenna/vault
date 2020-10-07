namespace ChainLib.Models
{
    public interface IHashProvider
    {
        byte[] ComputeHashBytes(string any);
	    byte[] ComputeHashBytes(byte[] any);
		byte[] ComputeHashBytes(object instance);
        string ComputeHashString(object instance);
        string ComputeHashString(string any);
	    string ComputeHashString(byte[] any);

	    byte[] DoubleHash(object instance);
	    byte[] DoubleHash(byte[] any);
	    byte[] DoubleHash(byte[] one, byte[] two);
		byte[] DoubleHash(string any);
    }
}