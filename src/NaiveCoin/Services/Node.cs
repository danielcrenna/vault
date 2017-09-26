using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using NaiveChain;
using NaiveCoin.Models;
using Newtonsoft.Json;

namespace NaiveCoin.Services
{
    public class Node
    {
        private readonly string _host;
        private readonly int _port;
        private readonly ICollection<Node> _peers;
        private readonly Blockchain _blockchain;
        private readonly JsonSerializerSettings _jsonSettings;
        private readonly ILogger<Node> _logger;
        private readonly HttpClient _http;

        private string Url => $"http://{_host}:{_port}";

        public IEnumerable<Node> Peers => _peers;

        public Node(string host, int port, Blockchain blockchain, JsonSerializerSettings jsonSettings, ILogger<Node> logger, IEnumerable<string> peers)
        {
            _host = host;
            _port = port;
            if (peers != null)
            {
                _peers = new HashSet<Node>(peers.Select(x =>
                {
                    var url = new Uri(x);
                    return new Node(url.Host, url.Port, blockchain, jsonSettings, logger, null);
                }));
            }
            _blockchain = blockchain;
            _jsonSettings = jsonSettings;
            _logger = logger;

            _http = new HttpClient();
        }

        public async Task ConnectToPeersAsync(params Node[] newPeers)
        {
            foreach (var peer in newPeers)
            {
                if (peer.Url == Url)
                    continue;

                // If it already has that peer, ignore.
                var hasPeer = false;
                if (_peers.Any(existing => existing.Url == peer.Url))
                {
                    _logger?.LogInformation($"Peer {peer.Url} not added to connections, because I already have it.");
                    hasPeer = true;
                }
                if (hasPeer)
                    continue;
                
                await SendPeerAsync(peer, this);
                _logger?.LogInformation($"Peer {peer.Url} added to connections.");
                _peers.Add(peer);

                await InitConnectionAsync(peer);
                Broadcast(async node => await SendPeerAsync(peer, node));
            }
        }

        public async Task SendPeerAsync(Node peer, Node peerToSend)
        {
            var url = $"{peer.Url}/node/peers";
            _logger.LogInformation($"Sending {peerToSend.Url} to peer {url}.");

            try
            {
                var response = await _http.PostAsync(url, null);
                var json = await response.Content.ReadAsStringAsync();
                var block = JsonConvert.DeserializeObject<CurrencyBlock>(json, _jsonSettings);

				await CheckReceivedBlocksAsync(block);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, $"Unable to send me to peer {url}: {e.Message}");
            }
        }

        public async Task InitConnectionAsync(Node peer)
        {
            await GetLatestBlockAsync(peer);
            await GetTransactionsAsync(peer);
        }

        public async Task GetLatestBlockAsync(Node peer)
        {
            var url = $"{peer.Url}/blockchain/blocks/latest";
            _logger?.LogInformation($"Getting latest block from: {url}");

            try
            {
                var response = await _http.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                var block = JsonConvert.DeserializeObject<CurrencyBlock>(json, _jsonSettings);

				await CheckReceivedBlocksAsync(block);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, $"Unable to get latest block from {url}: {e.Message}");
            }
        }

        public async Task SendLatestBlockAsync(Node peer, Block block)
        {
            var url = $"{peer.Url}/blockchain/blocks/latest";
            _logger?.LogInformation($"Sending latest block to: {url}");

            try
            {
                var body = JsonConvert.SerializeObject(block, _jsonSettings);
                var response = await _http.PutAsync(url, new StringContent(body, Encoding.UTF8));
                if(!response.IsSuccessStatusCode)
                    throw new HttpOperationException($"Status code was {response.StatusCode}");
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, $"Unable to send latest block to {url}: {e.Message}");
            }
        }

        public async Task GetBlocksAsync(Node peer)
        {
            var url = $"{peer.Url}/blockchain/blocks";
            _logger?.LogInformation($"Getting blocks from: {url}");

            try
            {
                var response = await _http.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                var block = JsonConvert.DeserializeObject<CurrencyBlock>(json, _jsonSettings);

				await CheckReceivedBlocksAsync(block);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, $"Unable to get blocks from {url}: {e.Message}");
            }
        }

        public async Task SendTransactionAsync(Node peer, Transaction transaction)
        {
            var url = $"{peer.Url}/blockchain/transactions";
            _logger?.LogInformation($"Sending transaction '{transaction.Id}' to: {url}");

            try
            {
                var body = JsonConvert.SerializeObject(transaction, _jsonSettings);
                var response = await _http.PostAsync(url, new StringContent(body, Encoding.UTF8));
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Response was {response.StatusCode} - {response.ReasonPhrase}");
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, $"Unable to send transaction to {url}: {e.Message}");
            }
        }

        public async Task GetTransactionsAsync(Node peer)
        {
            var url = $"{peer.Url}/blockchain/transactions";
            _logger?.LogInformation($"Getting transactions from: {url}");

            try
            {
                var response = await _http.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                var transactions = JsonConvert.DeserializeObject<IEnumerable<Transaction>>(json, _jsonSettings);

				await SyncTransactionsAsync(transactions);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, $"Unable to get blocks from {url}: {e.Message}");
            }
        }

        public async Task<bool> GetConfirmationAsync(Node peer, string transactionId)
        {
            var url = $"{peer.Url}/blockchain/blocks/transactions/{transactionId}";
            _logger?.LogInformation($"Getting confirmation from: {url}for transaction '{transactionId}'");

            try
            {
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Response was {response.StatusCode} - {response.ReasonPhrase}");
                return true;
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, $"Unable to get blocks from {url}: {e.Message}");
                return false;
            }
        }

        public async Task<int> GetConfirmationsAsync(string transactionId)
        {
            // Get from all peers if the transaction has been confirmed
            var foundLocally = await _blockchain.GetTransactionFromBlocksAsync(transactionId) != null;
            var confirmed = 0;
            foreach (var peer in _peers)
                confirmed += await GetConfirmationAsync(peer, transactionId) ? 1 : 0;
            return (foundLocally ? 1 : 0) + confirmed;
        }
        
        public void Broadcast(Action<Node> closure)
        {
            foreach (var peer in _peers)
            {
                closure(peer);
            }
        }

        private async Task SyncTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            // For each received transaction check if we have it, if not, add.
            foreach (var transaction in transactions)
            {
                var transactionFound = await _blockchain.GetTransactionByIdAsync(transaction.Id);
                if (transactionFound == null)
                {
                    _logger?.LogInformation($"Syncing transaction '{transaction.Id}");
                    _blockchain.AddTransaction(transaction);
                }
            }
        }

        public async Task<bool?> CheckReceivedBlocksAsync(params CurrencyBlock[] blocks)
        {
            var receivedBlocks = blocks.OrderBy(x => x.Index).ToList();
            var latestBlockReceived = receivedBlocks[receivedBlocks.Count - 1];
            var latestBlockHeld = await _blockchain.GetLastBlockAsync();

            // If the received blockchain is not longer than blockchain. Do nothing.
            if (latestBlockReceived.Index <= latestBlockHeld.Index)
            {
                _logger?.LogInformation($"Received blockchain is not longer than blockchain. Do nothing.");
                return false;
            }
            
            _logger?.LogInformation($"Blockchain possibly behind. We got: {latestBlockHeld.Index}, Peer got: {latestBlockReceived.Index}");

            if (latestBlockHeld.Hash == latestBlockReceived.PreviousHash)
            {
                // We can append the received block to our chain
                _logger?.LogInformation("Appending received block to our chain");
                await _blockchain.AddBlockAsync(latestBlockReceived);
                return true;
            }

            if (receivedBlocks.Count == 1)
            {
                // We have to query the chain from our peer
                _logger?.LogInformation("Querying chain from our peers");
                Broadcast(async node => await GetBlocksAsync(node));
                return null;
            }

            // Received blockchain is longer than current blockchain
            _logger?.LogInformation("Received blockchain is longer than current blockchain");
            await _blockchain.ReplaceChainAsync(receivedBlocks);
            return true;
        }
    }
}
