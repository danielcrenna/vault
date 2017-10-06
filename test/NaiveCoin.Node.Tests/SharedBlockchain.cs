using NaiveCoin.Tests.Fixtures;
using Xunit;

namespace NaiveCoin.Tests
{
	[CollectionDefinition("Blockchain")]
	public class SharedBlockchain : ICollectionFixture<BlockchainFixture> { }
}