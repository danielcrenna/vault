using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaiveCoin.DataAccess;
using NaiveCoin.Models;
using NaiveCoin.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NaiveCoin.Core.Providers;
using NaiveCoin.Wallets;

namespace NaiveCoin
{
    public class Dependencies
    {
        private const string Namespace = "NaiveCoin";

        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            // Settings:
            services.Configure<CoinSettings>(configuration.GetSection("Currency"));

            // Providers:
            {
                services.AddSingleton(r => new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter()
                    }
                });
                services.AddSingleton<ITransactionDataSerializer>(r => new JsonTransactionDataSerializer(r.GetRequiredService<JsonSerializerSettings>()));
                services.AddSingleton<IHashProvider>(r => new StableHashProvider(SHA512.Create()));

                var provider = new BrainWalletProvider();
                services.AddSingleton<IWalletProvider>(r => provider);
                services.AddSingleton<IWalletAddressProvider>(r => provider);
                services.AddSingleton<IWalletSecretProvider>(r => provider);

                services.AddSingleton<IProofOfWork>(r =>
                    new SimpleProofOfWork(r.GetRequiredService<IOptions<CoinSettings>>(), r.GetService<ILogger<SimpleProofOfWork>>()));
            }

            // Repositories:
            {
                services.AddScoped<IBlockRepository>(r => new SqliteBlockRepository(Namespace, "blockchain", r.GetService<ILogger<SqliteBlockRepository>>()));
                services.AddScoped<ITransactionRepository>(r => new SqliteTransactionRepository(Namespace, "blockchain", r.GetRequiredService<ITransactionDataSerializer>(), r.GetService<ILogger<SqliteTransactionRepository>>()));
                services.AddScoped<IWalletRepository>(r => new SqliteWalletRepository(Namespace, "wallets",
                    r.GetRequiredService<IWalletAddressProvider>(), 
                    r.GetService<ILogger<SqliteWalletRepository>>()));
            }

            // Services:
            {
                services.AddScoped(r => new Blockchain(
                    r.GetRequiredService<IOptions<CoinSettings>>(), 
                    r.GetRequiredService<IBlockRepository>(),
                    r.GetRequiredService<IProofOfWork>(),
                    r.GetRequiredService<ITransactionRepository>(), 
                    r.GetRequiredService<IHashProvider>(), 
                    r.GetRequiredService<JsonSerializerSettings>(), 
                    r.GetService<ILogger<Blockchain>>()));

                services.AddScoped(r => new Miner(
                    r.GetRequiredService<Blockchain>(),
                    r.GetRequiredService<IProofOfWork>(),
                    r.GetRequiredService<IOptions<CoinSettings>>(),
                    r.GetService<ILogger<Miner>>())
                );

                services.AddScoped(r => new Operator(
                    r.GetRequiredService<Blockchain>(),
                    r.GetRequiredService<IHashProvider>(),
                    r.GetRequiredService<IWalletProvider>(),
                    r.GetRequiredService<IWalletRepository>(),
                    r.GetRequiredService<IOptions<CoinSettings>>(),
                    r.GetService<ILogger<Operator>>()));

                services.AddScoped(r =>
                {
                    Uri url = null;
                    var commandLineUrls = configuration["urls"];
                    if (!string.IsNullOrWhiteSpace(commandLineUrls))
                        url = new Uri(commandLineUrls);

                    var peers = configuration.GetValue<string[]>("peers");
                    return new Node(
                        url?.Host ?? "localhost",
                        url?.Port ?? 5001,
                        r.GetRequiredService<Blockchain>(),
                        r.GetRequiredService<JsonSerializerSettings>(),
                        r.GetRequiredService<ILogger<Node>>(),
                        peers);
                });
            }
           
        }
    }
}
