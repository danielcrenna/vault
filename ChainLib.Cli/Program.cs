using System;
using System.Threading.Tasks;
using ChainLib.Crypto;
using ChainLib.Extensions;
using ChainLib.Models;
using ChainLib.Services;
using ChainLib.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChainLib.Cli
{
    class Program
    {
	    static void Main(string[] args)
	    {
		    IConfiguration config = new ConfigurationBuilder()
			    .AddEnvironmentVariables()
			    .AddJsonFile("chain.json")
			    .Build();

		    var loggerFactory = new LoggerFactory();
			loggerFactory.AddConsole();

		    var chain = new ChainSettings();
		    config.Bind(chain);

			var logger = loggerFactory.CreateLogger(chain.Name);

		    TaskScheduler.UnobservedTaskException += (s, e) =>
		    {
				UnhandledException(logger, e.Exception);
			};

		    AppDomain.CurrentDomain.UnhandledException += (s, e) =>
		    {
				UnhandledException(logger, e.ExceptionObject as Exception);
			};

		    try
		    {
			    var hashProvider = new ObjectHashProvider();
			    var typeProvider = new BlockObjectTypeProvider();

			    if (chain.GenesisBlock == null)
				{
					logger.LogCritical("Chain genesis block is missing");
					goto failFast;
				}

				if (chain.GenesisBlock.Hash == null)
			    {
				    logger.LogCritical("Chain genesis block hash is missing");
				    goto failFast;
			    }

			    var genesisHash = chain.GenesisBlock.ToHashBytes(hashProvider);

#if DEBUG
			    var genesisHashDebug = "[";
			    foreach (var c in genesisHash)
				    genesisHashDebug += c + ", ";
			    genesisHashDebug += "]";
			    logger.LogDebug("Expected genesisHash = " + genesisHashDebug);
#endif

				if (!chain.GenesisBlock.Hash.ConstantTimeEquals(genesisHash))
				{
					logger.LogCritical($"Chain genesis block hash '{genesisHash.ToHex()}' is invalid");
					goto failFast;
				}

			    var chainHash = hashProvider.ComputeHashBytes(chain);

#if DEBUG
				var chainHashDebug = "[";
				foreach (var c in chainHash)
					chainHashDebug += c + ", ";
				chainHashDebug += "]";
			    logger.LogDebug("Expected chainHash = " + chainHashDebug);
#endif

				if (!chain.Hash.ConstantTimeEquals(chainHash))
			    {
					logger.LogCritical("Chain configuration hash is missing or invalid");
					goto failFast;
			    }
				
			    //
			    // Storage:
			    //
			    IBlockRepository blocks = null;
			    if (!string.IsNullOrWhiteSpace(chain.StorageEngine))
			    {
				    switch (chain.StorageEngine?.ToUpperInvariant())
				    {
					    case "SQLITE":
					    {
						    string baseDirectory = !string.IsNullOrWhiteSpace(chain.StorageDirectory)
							    ? chain.StorageEngine
							    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

						    blocks = new SqliteBlockRepository(baseDirectory, chain.Name, "blocks", chain.GenesisBlock, hashProvider,
							    typeProvider, loggerFactory.CreateLogger<SqliteBlockRepository>());
						    break;
					    }
				    }
			    }

			    //
			    // Blockchain:
			    //
			    IBlockchain blockchain;
			    if (blocks == null)
			    {
				    logger.LogCritical("Could not find a supported storage engine for the chain.");
				    goto failFast;
				}

			    blockchain = new Blockchain(blocks, new NoProofOfWork(), hashProvider,
				    loggerFactory.CreateLogger<Blockchain>());

			    logger.LogInformation($"{chain.Name} is running.");
		    }
		    catch (Exception ex)
		    {
			    UnhandledException(logger, ex);
		    }
			
			failFast:

		    if (Environment.UserInteractive)
		    {
			    logger.LogInformation("Press any key to shut down.");
			    Console.ReadKey();
			}
		}

	    private static void UnhandledException(ILogger logger, Exception exception)
	    {
		    logger.LogCritical(exception, $"Unexpected error");
	    }
    }
}
