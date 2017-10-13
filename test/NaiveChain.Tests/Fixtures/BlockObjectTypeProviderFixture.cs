using NaiveChain.Models;

namespace NaiveChain.Tests.Fixtures
{
	public class BlockObjectTypeProviderFixture
	{
		public BlockObjectTypeProviderFixture()
		{
			Value = new BlockObjectTypeProvider();
		}

		public IBlockObjectTypeProvider Value { get; set; }
	}
}