using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChainLib.Models;
using ChainLib.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Newtonsoft.Json;

namespace ChainLib.Node
{
    public class Peer
    {
        private readonly string _host;
        private readonly int _port;
        private readonly ICollection<Peer> _peers;
        private readonly Blockchain _blockchain;
        private readonly JsonSerializerSettings _jsonSettings;
        private readonly ILogger<Peer> _logger;
        private readonly HttpClient _http;

        private string Url => $"http://{_host}:{_port}";

        public IEnumerable<Peer> Peers => _peers;

        public Peer(string host, int port, Blockchain blockchain, JsonSerializerSettings jsonSettings, ILogger<Peer> logger, IEnumerable<string> peers)
        {
            _host = host;
            _port = port;
            if (peers != null)
            {
                _peers = new HashSet<Peer>(peers.Select(x =>
                {
                    var url = new Uri(x);
                    return new Peer(url.Host, url.Port, blockchain, jsonSettings, logger, null);
                }));
            }
            _blockchain = blockchain;
            _jsonSettings = jsonSettings;
            _logger = logger;

            _http = new HttpClient();
        }

        public async Task ConnectToPeersAsync(params Peer[] newPeers)
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

        public async Task SendPeerAsync(Peer peer, Peer peerToSend)
        {
            var url = $"{peer.Url}/node/peers";
            _logger.LogInformation($"Sending {peerToSend.Url} to peer {url}.");

            try
            {
                var response = await _http.PostAsync(url, null);
                var json = await response.Content.ReadAsStringAsync();
                var block = JsonConvert.DeserializeObject<Block>(json, _jsonSettings);

				await CheckReceivedBlocksAsync(block);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, $"Unable to send me to peer {url}: {e.Message}");
            }
        }

        public async Task InitConnectionAsync(Peer peer)
        {
            await GetLatestBlockAsync(peer);
        }

        public async Task GetLatestBlockAsync(Peer peer)
        {
            var url = $"{peer.Url}/blockchain/blocks/latest";
            _logger?.LogInformation($"Getting latest block from: {url}");

            try
            {
                var response = await _http.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                var block = JsonConvert.DeserializeObject<Block>(json, _jsonSettings);

				await CheckReceivedBlocksAsync(block);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, $"Unable to get latest block from {url}: {e.Message}");
            }
        }

        public async Task SendLatestBlockAsync(Peer peer, Block block)
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

        public async Task GetBlocksAsync(Peer peer)
        {
            var url = $"{peer.Url}/blockchain/blocks";
            _logger?.LogInformation($"Getting blocks from: {url}");

            try
            {
                var response = await _http.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                var block = JsonConvert.DeserializeObject<Block>(json, _jsonSettings);

				await CheckReceivedBlocksAsync(block);
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, $"Unable to get blocks from {url}: {e.Message}");
            }
        }

        public void Broadcast(Action<Peer> closure)
        {
            foreach (var peer in _peers)
            {
                closure(peer);
            }
        }

        public async Task<bool?> CheckReceivedBlocksAsync(params Block[] blocks)
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
