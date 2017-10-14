using hq.pipes.Producers;
using NaiveChain.Models;

namespace NaiveChain.Streaming
{
	public class BlockObjectProducer<T> : BackgroundProducer<T> where T : IBlockSerialized
	{
		public BlockObjectProducer(BlockObjectProjection projection)
		{
			Background.Produce(projection.StreamForwards<T>());
		}
	}
}