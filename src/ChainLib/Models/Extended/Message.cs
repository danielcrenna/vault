using ChainLib.Serialization;

namespace ChainLib.Models.Extended
{
	public class Message : IBlockSerialized
	{
		public string Text { get; set; }

		public Message(string text)
		{
			Text = text; }

		public void Serialize(BlockSerializeContext context)
		{
			context.bw.Write(Text);
		}

		public Message(BlockDeserializeContext context)
		{
			Text = context.br.ReadString();
		}
	}
}