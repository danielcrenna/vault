using ChainLib.Models;
using reactive.pipes.Producers;

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