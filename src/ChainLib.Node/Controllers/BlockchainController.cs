using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ChainLib.Crypto;
using ChainLib.Models;
using ChainLib.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChainLib.Node.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Controls operations on the underlying blockchain.
    /// </summary>
    [Route("blockchain")]
    public class BlockchainController : Controller
    {
        private readonly Peer _node;
        private readonly Blockchain _blockchain;

        public BlockchainController(Peer node, Blockchain blockchain)
        {
            _node = node;
            _blockchain = blockchain;
        }

        /// <summary>
        /// Streams a complete copy of this node's blockchain to calling clients.
        /// </summary>
        /// <returns></returns>
        [HttpGet("blocks")]
        public IActionResult StreamAllBlocks(bool forwards, long startingAt = 0L)
        {
            var blocks = _blockchain.StreamAllBlocks(forwards, startingAt: startingAt);
            if (!blocks.Any())
                return NotFound();

            return Ok(blocks);
        }

        /// <summary>
        /// Gets the last block on the blockchain, according to this node.
        /// </summary>
        /// <returns></returns>
        [HttpGet("blocks/latest")]
        public async Task<IActionResult> GetLastBlock()
        {
            var last = await _blockchain.GetLastBlockAsync();
            if (last == null)
            {
                return NotFound(new
                {
                    Message = "Last block not found"
                });
            }

            return Ok(last);
        }

        /// <summary>
        /// Attempt to append the chain with the provided block. Used as a mechanism to sync peers. 
        /// </summary>
        [HttpPut("blocks/latest")]
        public async Task<IActionResult> VerifyLastBlock([FromBody] Block block)
        {
            var result = await _node.CheckReceivedBlocksAsync(block);
            if (result == null)
            {
                return Accepted(new
                {
                    Message = "Requesting blockchain to check."
                });
            }

            if (result.Value)
            {
                return Ok(block);
            }

            return StatusCode((int) HttpStatusCode.Conflict, new
            {
                Message = "Blockchain is up to date."
            });
        }

        /// <summary>
        /// Retrieve a block by hash.
        /// </summary>
        [HttpGet("blocks/{hash}")]
        public async Task<IActionResult> GetBlockByHash(string hash)
        {
            var blockFound = await _blockchain.GetBlockByHashAsync(hash.FromHex());
            if (blockFound == null)
                return NotFound(new
                {
                    Message = $"Block not found with hash '{hash}'"
                });

            return Ok(blockFound);
        }

        /// <summary>
        /// Retrieve a block by index.
        /// </summary>
        [HttpGet("blocks/{index}")]
        public async Task<IActionResult> GetBlockByIndex(long index)
        {
            var blockFound = await _blockchain.GetBlockByIndexAsync(index);
            if (blockFound == null)
                return NotFound(new
                {
                    Message = $"Block not found with index '{index}'"
                });

            return Ok(blockFound);
        }
    }
}
