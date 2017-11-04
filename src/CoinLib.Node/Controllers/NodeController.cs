using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CoinLib.Node.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Controls operations for peer connections.
    /// </summary>
    [Route("node")]
    public class NodeController : Controller
    {
        private readonly Services.Peer _self;

        public NodeController(Services.Peer self)
        {
            _self = self;
        }

        /// <summary>
        /// Retrieves all peers for this node.
        /// </summary>
        /// <returns></returns>
        [HttpGet("peers")]
        public IActionResult GetPeers()
        {
            return Ok(_self.Peers);
        }

        /// <summary>
        /// Subscribe a new peer to this node.
        /// </summary>
        /// <param name="peer"></param>
        /// <returns></returns>
        [HttpPost("peers")]
        public IActionResult AddPeer([FromBody] Services.Peer peer)
        {
            var newPeer = _self.ConnectToPeersAsync(peer);

            return Created("node/peers", newPeer);
        }

        /// <summary>
        /// Retrieve the count of all peers that have confirmed the given transaction.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        [HttpGet("transactions/{transactionId}/confirmations")]
        public async Task<IActionResult> GetConfirmationsAsync([FromRoute]string transactionId)
        {
            var confirmations = await _self.GetConfirmationsAsync(transactionId);

            return Ok(new
            {
                Confirmations = confirmations
            });
        }
    }
}