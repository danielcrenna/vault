using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using NaiveCoin.Models;
using NaiveCoin.Models.Exceptions;
using NaiveCoin.Services;

namespace NaiveCoin.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Controls operations on the underlying blockchain.
    /// </summary>
    [Route("blockchain")]
    public class BlockchainController : Controller
    {
        private readonly Node _node;
        private readonly Blockchain _blockchain;

        public BlockchainController(Node node, Blockchain blockchain)
        {
            _node = node;
            _blockchain = blockchain;
        }

        /// <summary>
        /// Streams a complete copy of this node's blockchain to calling clients.
        /// </summary>
        /// <returns></returns>
        [HttpGet("blocks")]
        public IActionResult GetAllBlocks()
        {
            var blocks = _blockchain.GetAllBlocks();
            if (!blocks.Any())
                return NotFound();

            return Ok(blocks);
        }

        /// <summary>
        /// Gets the last block on the blockchain, according to this node.
        /// </summary>
        /// <returns></returns>
        [HttpGet("blocks/latest")]
        public IActionResult GetLastBlock()
        {
            var last = _blockchain.GetLastBlock();
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
        public IActionResult VerifyLastBlock([FromBody]Block block)
        {
            var result = _node.CheckReceivedBlocks(block);
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
        public IActionResult GetBlockByHash(string hash)
        {
            var blockFound = _blockchain.GetBlockByHash(hash);
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
        public IActionResult GetBlockByIndex(long index)
        {
            var blockFound = _blockchain.GetBlockByIndex(index);
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
        public IActionResult GetTransactionFromBlocks(string transactionId)
        {
            var blockFound = _blockchain.GetTransactionFromBlocks(transactionId);
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
            var transactions = _blockchain.GetAllTransactions();
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
        public IActionResult AddTransaction([FromBody] Transaction transaction)
        {
            var transactionFound = _blockchain.GetTransactionById(transaction.Id);
            if(transactionFound != null)
                return StatusCode((int)HttpStatusCode.Conflict, new
                {
                    Message = $"Transaction '{transaction.Id}' already exists"
                });

            try
            {
                Transaction newTransaction = _blockchain.AddTransaction(transaction);

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
        public IActionResult GetUnspentTransactionsForAddress([FromRoute]string address)
        {
            return Ok(_blockchain.GetUnspentTransactionsForAddress(address));
        }
    }
}
