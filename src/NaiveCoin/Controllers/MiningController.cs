using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NaiveCoin.Models.Exceptions;
using NaiveCoin.Services;

namespace NaiveCoin.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Controls operations for mining new coins.
    /// </summary>
    [Route("miner")]
    public class MiningController : Controller
    {
        private readonly Blockchain _blockchain;
        private readonly Miner _miner;

        public MiningController(Blockchain blockchain, Miner miner)
        {
            _blockchain = blockchain;
            _miner = miner;
        }

        [HttpPost("mine/{rewardAddress}")]
        public async Task<IActionResult> Mine(string rewardAddress)
        {
            try
            {
                var newBlock = await _miner.MineAsync(rewardAddress);
                var block = await _blockchain.AddBlockAsync(newBlock);
                return Created($"blockchain/blocks/{block.Index}", block);
            }
            catch (Exception ex)
            {
                if (ex is BlockAssertionException && ex.Message.StartsWith("Invalid index"))
                {
                    return StatusCode((int) HttpStatusCode.Conflict,
                        "A new block was added before we were able to mine one");
                }

                throw;
            }
        }
    }
}
