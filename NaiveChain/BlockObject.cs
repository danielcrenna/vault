namespace NaiveChain
{
	public class BlockObject
	{
		public long BlockIndex { get; set; }
		public int Index { get; set; }
		public long Timestamp { get; set; }
		public byte[] Hash { get; set; }
		public object Data { get; set; }
	}
}