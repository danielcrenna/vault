using System.Collections.Generic;
using System.Diagnostics;
using ChainLib.Serialization;

namespace ChainLib.Models.Extended
{
	public class Transaction : IBlockSerialized
    {
		public enum TransactionType : byte { Unknown, Regular, Fee, Reward }

		public string Id { get; set; }
	    public TransactionType Type { get; set; }
	    public TransactionData Data { get; set; }
		
	    public Transaction() { }

	    public void Serialize(BlockSerializeContext context)
	    {
		    context.bw.WriteNullableString(Id);
		    context.bw.Write((byte)Type);

		    if (context.bw.WriteBoolean(Data != null))
		    {
				Debug.Assert(Data != null);
			    SerializeTransactionItems(context, Data.Inputs);
			    SerializeTransactionItems(context, Data.Outputs);
			}
	    }

	    private static void SerializeTransactionItems(BlockSerializeContext context, ICollection<TransactionItem> items)
	    {
		    if (context.bw.WriteBoolean(items != null))
		    {
			    Debug.Assert(items != null);
			    context.bw.Write(items.Count);
			    foreach (var input in items)
			    {
				    context.bw.Write(input.Index);
				    context.bw.Write(input.TransactionId);
				    context.bw.Write((byte) input.Type);
				    context.bw.WriteBuffer(input.Address);
				    context.bw.Write(input.Amount);
				    context.bw.WriteBuffer(input.Signature);
			    }
		    }
	    }

	    public Transaction(BlockDeserializeContext context)
	    {
			Id = context.br.ReadNullableString();
		    Type = (TransactionType)context.br.ReadByte();

		    if (context.br.ReadBoolean())
		    {
			    var inputs = DeserializeTransactionItems(context);
			    var outputs = DeserializeTransactionItems(context);

				Data = new TransactionData
			    {
				    Inputs = inputs,
				    Outputs = outputs
			    };
		    }
	    }

		private static IList<TransactionItem> DeserializeTransactionItems(BlockDeserializeContext context)
	    {
		    var list = new List<TransactionItem>();

		    if (context.br.ReadBoolean())
		    {
			    var count = context.br.ReadInt32();
			    for(var i = 0; i < count; i++)
			    {
				    var item = new TransactionItem
				    {
					    Index = context.br.ReadInt64(),
					    TransactionId = context.br.ReadString(),
					    Type = (TransactionDataType) context.br.ReadByte(),
					    Address = context.br.ReadBuffer(),
					    Amount = context.br.ReadInt64(),
					    Signature = context.br.ReadBuffer()
				    };

					list.Add(item);
			    }
		    }

		    return list;
	    }
	}
}
