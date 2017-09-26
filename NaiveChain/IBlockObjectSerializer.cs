namespace NaiveChain
{
    public interface IBlockObjectSerializer
    {
        byte[] Serialize(BlockObject @object);
	    BlockObject Deserialize(byte[] json);
    }
}