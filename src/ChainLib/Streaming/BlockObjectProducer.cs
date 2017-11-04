using hq.pipes.Producers;
using ChainLib.Models;

namespace ChainLib.Streaming
{
	public class BlockObjectProducer<T> : BackgroundProducer<T> where T : IBlockSerialized
	{
		public BlockObjectProducer(BlockObjectProjection projection)
		{
			Background.Produce(projection.Stream<T>());
		}
	}
}