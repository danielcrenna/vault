using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainLib.Crypto;
using ChainLib.Extensions;
using ChainLib.Models;
using ChainLib.Models.Extended;
using ChainLib.Streaming;
using ChainLib.Tests.Fixtures;
using Xunit;

namespace ChainLib.Tests
{
	public class WithUnencryptedBlockDatabase : WithBlockDatabase<EmptyBlockRepositoryFixture>,
		IClassFixture<ObjectHashProviderFixture>, IClassFixture<EmptyBlockRepositoryFixture>
	{
		public WithUnencryptedBlockDatabase(EmptyBlockRepositoryFixture blockDatabase, ObjectHashProviderFixture hashProvider) : base(blockDatabase, hashProvider)
		{

		}
	}

	public class WithEncryptedBlockDatabase : 
		WithBlockDatabase<EncryptedEmptyBlockRepositoryFixture>,
		IClassFixture<ObjectHashProviderFixture>, IClassFixture<EncryptedEmptyBlockRepositoryFixture>
	{
		public WithEncryptedBlockDatabase(
			EncryptedEmptyBlockRepositoryFixture blockDatabase, 
			ObjectHashProviderFixture hashProvider) : base(blockDatabase, hashProvider)
		{

		}
	}

	public abstract class WithBlockDatabase<T>
		where T : EmptyBlockRepositoryFixture
	{
		protected WithBlockDatabase(EmptyBlockRepositoryFixture blockDatabase, ObjectHashProviderFixture hashProvider)
		{
			Fixture = blockDatabase;
			TypeProvider = Fixture.TypeProvider;
			HashProvider = hashProvider.Value;

			TypeProvider.TryAdd(0, typeof(Transaction));
		}

		public IHashProvider HashProvider { get; set; }
		public EmptyBlockRepositoryFixture Fixture { get; set; }
		public IBlockObjectTypeProvider TypeProvider { get; }

		[Fact]
		public async Task Can_stream_typed_objects_and_headers()
		{
			await Fixture.Value.AddAsync(CreateBlock());

			var objectProjection = new BlockObjectProjection(Fixture.Value, TypeProvider);
			var objectStream = objectProjection.Stream<Transaction>();

			Assert.NotNull(objectStream);
			Assert.Single(objectStream);

			var headerStream = Fixture.Value.StreamAllBlockHeaders(true, 2);
			Assert.NotNull(headerStream);
			Assert.Single(headerStream);
		}

		private Block CreateBlock()
		{
			var transaction = new Transaction
			{
				Id = $"{Guid.NewGuid()}"
			};
			var blockObject = new BlockObject
			{
				Data = transaction
			};
			blockObject.Hash = blockObject.ToHashBytes(HashProvider);

			var block = new Block
			{
				Nonce = 1,
				PreviousHash = "rosebud".Sha256(),
				MerkleRootHash = HashProvider.DoubleHash(blockObject.Hash),
				Timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
				Objects = new List<BlockObject> { blockObject }
			};
			block.MerkleRootHash = block.ComputeMerkleRoot(HashProvider);
			block.Hash = block.ToHashBytes(HashProvider);
			return block;
		}
	}
}