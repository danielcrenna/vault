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
                services.AddSingleton<IObjectHashProvider>(r => new StableObjectHashProvider(SHA512.Create()));
                services.AddSingleton<IWalletProvider>(r => new BrainWalletProvider());
            }

            // Repositories:
            {
                services.AddScoped<IBlockRepository>(r => new SqliteBlockRepository(Namespace, r.GetService<ILogger<SqliteBlockRepository>>()));
                services.AddScoped<ITransactionRepository>(r => new SqliteTransactionRepository(Namespace, r.GetRequiredService<ITransactionDataSerializer>(), r.GetService<ILogger<SqliteTransactionRepository>>()));
                services.AddScoped<IWalletRepository>(r => new SqliteWalletRepository(Namespace, r.GetRequiredService<IWalletProvider>(), r.GetService<ILogger<SqliteWalletRepository>>()));
            }

            // Services:
            {
                services.AddScoped(r => new Blockchain(
                    r.GetRequiredService<IOptions<CoinSettings>>().Value, 
                    r.GetRequiredService<IBlockRepository>(), 
                    r.GetRequiredService<ITransactionRepository>(), 
                    r.GetRequiredService<IObjectHashProvider>(), 
                    r.GetRequiredService<JsonSerializerSettings>(), 
                    r.GetService<ILogger<Blockchain>>()));

                services.AddScoped(r => new Miner(
                    r.GetRequiredService<Blockchain>(),
                    r.GetRequiredService<IObjectHashProvider>(),
                    r.GetRequiredService<IOptions<CoinSettings>>(),
                    r.GetService<ILogger<Miner>>())
                );

                services.AddScoped(r => new Operator(
                    r.GetRequiredService<Blockchain>(),
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
