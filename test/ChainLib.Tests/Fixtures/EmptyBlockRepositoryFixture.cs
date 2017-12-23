using System;
using Microsoft.Extensions.Logging;
using ChainLib.Extensions;
using ChainLib.Models;
using ChainLib.Sqlite;

namespace ChainLib.Tests.Fixtures
{
	public class EmptyBlockRepositoryFixture : IDisposable
	{
		public EmptyBlockRepositoryFixture()
		{
			var @namespace = $"{Guid.NewGuid()}";

			Init(@namespace);
		}

		protected void Init(string subDirectory)
		{
			var hashProvider = new ObjectHashProviderFixture().Value;
			var typeProvider = new BlockObjectTypeProviderFixture().Value;
			var factory = new LoggerFactory();
			factory.AddConsole();

			var genesisBlock = new Block
			{
				Index = 0L,
				PreviousHash = new byte[] {0},
				Timestamp = 1465154705L,
				Nonce = 0L,
				Objects = new BlockObject[] { },
			};

			var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			Value = new SqliteBlockRepository(
				baseDirectory,
				subDirectory,
				"blockchain",
				genesisBlock,
				hashProvider,
				typeProvider,
				factory.CreateLogger<SqliteBlockRepository>());

			genesisBlock.Index = 1;

			genesisBlock.Hash = genesisBlock.ToHashBytes(hashProvider);

			Value.AddAsync(genesisBlock).ConfigureAwait(false).GetAwaiter().GetResult();
			
			GenesisBlock = genesisBlock;

			TypeProvider = typeProvider;
		}

		public SqliteBlockRepository Value { get; set; }

		public Block GenesisBlock { get; private set; }

		public IBlockObjectTypeProvider TypeProvider { get; private set; }

		public void Dispose()
		{
			Value.Purge();
		}
	}
}