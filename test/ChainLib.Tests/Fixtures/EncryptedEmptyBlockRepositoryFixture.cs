using System;

namespace ChainLib.Tests.Fixtures
{
	public class EncryptedEmptyBlockRepositoryFixture : EmptyBlockRepositoryFixture
	{
		public EncryptedEmptyBlockRepositoryFixture() : base(
			$"{Guid.NewGuid()}",
			new ObjectHashProviderFixture().Value,
			new EncryptedBlockObjectTypeProviderFixture().Value)
		{ }
	}
}