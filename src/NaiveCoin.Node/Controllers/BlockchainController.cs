using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NaiveCoin.Core;
using NaiveCoin.Models;
using NaiveCoin.Models.Exceptions;
using NaiveCoin.Node.Services;

namespace NaiveCoin.Node.Controllers
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
        public IActionResult StreamAllBlocks()
        {
            var blocks = _blockchain.StreamAllBlocks();
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
        public async Task<IActionResult> VerifyLastBlock([FromBody]CurrencyBlock block)
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

        /// <summary>
        /// Retrieves a block transaction by its ID.
        /// </summary>
        [HttpGet("blocks/transactions/{transactionId}")]
        public async Task<IActionResult> GetTransactionFromBlocks(string transactionId)
        {
            var blockFound = await _blockchain.GetTransactionFromBlocksAsync(transactionId);
            if (blockFound == null)
            {
                return NotFound(new
                {
                    Message = $"Transaction '${transactionId}' not found in any block"
                });
            }

            return Ok(blockFound);
        }

        /// <summary>
        /// Retrieves all pending transactions. Pending transactions are not currently in a block.
        /// </summary>
        /// <returns></returns>
        [HttpGet("transactions")]
        public IActionResult GetAllTransactions()
        {
            var transactions = _blockchain.StreamAllTransactions();
            if (!transactions.Any())
                return NotFound();
            return Ok(transactions);
        }

        /// <summary>
        /// Adds a new pending transaction.
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        [HttpPost("transactions")]
        public async Task<IActionResult> AddTransaction([FromBody] Transaction transaction)
        {
            var transactionFound = await _blockchain.GetTransactionByIdAsync(transaction.Id);
            if(transactionFound != null)
                return StatusCode((int)HttpStatusCode.Conflict, new
                {
                    Message = $"Transaction '{transaction.Id}' already exists"
                });

            try
            {
                Transaction newTransaction = await _blockchain.AddTransactionAsync(transaction);

                return Ok(newTransaction);
            }
            catch (Exception ex)
            {
                if (ex is TransactionAssertionException)
                {
                    return BadRequest(new
                    {
                        ex.Message
                    });
                }
                throw;
            }
        }

        [HttpGet("transactions/unspent/{address}")]
        public async Task<IActionResult> GetUnspentTransactionsForAddress([FromRoute]string address)
        {
	        var model = await _blockchain.GetUnspentTransactionsForAddressAsync(address);

	        return Ok(model);
        }
    }
}
