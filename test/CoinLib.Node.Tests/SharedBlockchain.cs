using CoinLib.Tests.Fixtures;
using Xunit;

namespace CoinLib.Tests
{
	[CollectionDefinition("Blockchain")]
	public class SharedBlockchain : ICollectionFixture<BlockchainFixture> { }
}