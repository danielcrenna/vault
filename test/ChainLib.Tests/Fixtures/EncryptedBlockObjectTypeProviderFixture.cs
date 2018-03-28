using ChainLib.Crypto;
using ChainLib.Models;

namespace ChainLib.Tests.Fixtures
{
	public class EncryptedBlockObjectTypeProviderFixture
	{
		public EncryptedBlockObjectTypeProviderFixture()
		{
			Value = new BlockObjectTypeProvider(CryptoUtil.RandomBytes(32));
		}

		public IBlockObjectTypeProvider Value { get; set; }
	}
}