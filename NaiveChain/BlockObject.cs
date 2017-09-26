namespace NaiveChain
{
	public class BlockObject
	{
		public int Index { get; set; }
		public string SourceId { get; set; }
		public long Version { get; set; }
		public object Data { get; set; }
		public long Timestamp { get; set; }
		public byte[] Hash { get; set; }
	}
}