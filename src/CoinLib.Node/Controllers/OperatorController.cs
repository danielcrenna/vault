using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ChainLib.Models;
using CoinLib.Models;
using CoinLib.Models.Exceptions;
using CoinLib.Node.Services;
using CoinLib.Services;
using CoinLib.ViewModels;
using Crypto.Shim;

namespace CoinLib.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Controls operations for wallet management.
    /// </summary>
    [Route("operator")]
    public class OperatorController : Controller
    {
        private readonly Operator _operator;
        private readonly Blockchain _blockchain;
        private readonly IHashProvider _hashProvider;
        private readonly CoinSettings _coinSettings;

        public OperatorController(Operator @operator, Blockchain blockchain, IHashProvider hashProvider, IOptions<CoinSettings> coinSettings)
        {
            _operator = @operator;
            _blockchain = blockchain;
            _hashProvider = hashProvider;
            _coinSettings = coinSettings.Value;
        }

        /// <summary>
        /// Retrieves all wallets.
        /// </summary>
        [HttpGet("wallets")]
        public async Task<IActionResult> GetAll()
        {
	        var all = await _operator.GetWalletsAsync();

			var wallets = all.Select(wallet => new
            {
                wallet.Id,
                Addresses = wallet.GetAddresses()
            });

            if (!wallets.Any())
                return NotFound();

            return Ok(wallets);
        }

        /// <summary>
        /// Creates a new wallet using a given password as a seed.
        /// </summary>
        [HttpPost("wallets")]
        public IActionResult CreateWallet([FromBody] CreateWalletViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var matches = Regex.Matches(model.Password, @"\w+", RegexOptions.Compiled);
            if (matches.Count <= 4)
                return BadRequest("Password must contain at least five words");

            var wallet = _operator.CreateWalletFromPassword(model.Password);
            if (wallet == null)
                return NotFound();

            return Created($"operator/wallets/{wallet.Id}", new
            {
                wallet.Id,
                Addresses = wallet.GetAddresses()
            });
        }

        /// <summary>
        /// Retrieves a wallet by ID.
        /// </summary>
        [HttpGet("wallets/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var wallet = await _operator.GetWalletByIdAsync(id);
            if (wallet == null)
                return NotFound();

            return Ok(new
            {
                wallet.Id,
                Addresses = wallet.GetAddresses()
            });
        }

        /// <summary>
        /// Creates a new wallet transaction.
        /// </summary>
        [HttpPost("wallets/{id}/transactions")]
        public async Task<IActionResult> CreateTransaction([FromRoute] string id, [FromBody] CreateTransactionViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (await _operator.CheckWalletPasswordAsync(id, model.Password) == null)
                return NotFound();

            try
            {
                Transaction newTransaction = await _operator.CreateTransactionAsync(id,
                    model.FromAddress.FromHex(),
                    model.ToAddress.FromHex(), 
                    model.Amount,
                    model.ChangeAddress.FromHex()
                );

                newTransaction.Check(_hashProvider, _coinSettings);

                Transaction transaction = await _blockchain.AddTransactionAsync(newTransaction);

                return Created($"/operator/wallets/{id}/transactions", transaction);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException || ex is TransactionAssertionException)
                    return BadRequest(new
                    {
                        ex.Message
                    });

                throw;
            }
        }

        /// <summary>
        /// Retrieves all wallet addresses.
        /// </summary>
        [HttpGet("wallets/{id}/addresses")]
        public async Task<IActionResult> GetAddresses([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var wallet = await _operator.GetWalletByIdAsync(id);
            if (wallet == null)
                return NotFound();

            return Ok(new
            {
                Addresses = wallet.GetAddresses()
            });
        }

        /// <summary>
        /// Creates a new wallet address.
        /// </summary>
        [HttpPost("wallets/{id}/addresses")]
        public async Task<IActionResult> CreateAddress([FromRoute] string id, [FromBody] CreateAddressViewModel model)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            if (string.IsNullOrWhiteSpace(model?.Password))
                return BadRequest();

            var wallet = await _operator.CheckWalletPasswordAsync(id, model.Password);
            if (wallet == null)
                return NotFound();

            var address = wallet.KeyPairs[0].PublicKey;

            return Created($"operator/wallets/{wallet.Id}/addresses/{address}/balance", address);
        }

        /// <summary>
        /// Retrieves the balance of a given wallet and address.
        /// </summary>
        [HttpGet("wallets/{id}/addresses/{address}/balance")]
        public IActionResult GetAddressBalance([FromRoute] string id, [FromRoute] string address)
        {
            var balance = _operator.GetBalanceForWalletAddressAsync(id, address);
            return Ok(new
            {
                Balance = balance
            });
        }
    }
}