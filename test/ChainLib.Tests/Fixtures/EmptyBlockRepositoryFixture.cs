using System;
using Microsoft.Extensions.Logging;
using ChainLib.Extensions;
using ChainLib.Models;
using ChainLib.Sqlite;

namespace ChainLib.Tests.Fixtures
{
	public class EmptyBlockRepositoryFixture : IDisposable
	{
		public EmptyBlockRepositoryFixture() : this(
			$"{Guid.NewGuid()}", 
			new ObjectHashProviderFixture().Value, 
			new BlockObjectTypeProviderFixture().Value) { }

		protected EmptyBlockRepositoryFixture(string subDirectory, IHashProvider hashProvider, IBlockObjectTypeProvider typeProvider)
		{
			var factory = new LoggerFactory();
			factory.AddConsole();

			var genesisBlock = new Block
			{
				Index = 0L,
				PreviousHash = new byte[] {0},
				MerkleRootHash = new byte[] {0},
				Timestamp = 1465154705U,
				Nonce = 0L,
				Objects = new BlockObject[] { },
			};

			var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			Value = new SqliteBlockRepository(
				baseDirectory,
				subDirectory,
				"blockchain",
				genesisBlock,
				typeProvider,
				factory.CreateLogger<SqliteBlockRepository>());

			genesisBlock.Index = 1;

			genesisBlock.Hash = genesisBlock.ToHashBytes(hashProvider);

			Value.AddAsync(genesisBlock).ConfigureAwait(false).GetAwaiter().GetResult();
			
			GenesisBlock = genesisBlock;

			TypeProvider = typeProvider;
		}

		public SqliteBlockRepository Value { get; set; }

		public Block GenesisBlock { get; }

		public IBlockObjectTypeProvider TypeProvider { get; }

		public void Dispose()
		{
			Value.Purge();
		}
	}
}