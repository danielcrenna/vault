using System.Text;
using ChainLib.Models;

namespace ChainLib.Tests.Fixtures
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