using System;
using NaiveCoin.Core.Helpers;
using NaiveCoin.Tests.Fixtures;
using NaiveCoin.Wallets;
using Xunit;

namespace NaiveCoin.Tests
{
	[Collection("Blockchain")]
	public class EndToEnd : 
		IClassFixture<MinerFixture>,
		IClassFixture<OperatorFixture>
    {
	    private readonly BlockchainFixture _blockchain;
	    private readonly MinerFixture _miner;
	    private readonly OperatorFixture _operator;

		public EndToEnd(
			BlockchainFixture blockchain, 
			MinerFixture miner,
			OperatorFixture @operator)
	    {
		    _blockchain = blockchain;
		    _miner = miner;
		    _operator = @operator;
		}

		[Fact]
	    public async void We_can_mine_coins_and_send_them_somewhere()
		{
			// create a wallet!
			Wallet wallet1 = _operator.Value.CreateWalletFromPassword("purple monkey dishwasher");

			// save the wallet (this will add a default address)
			await _operator.Value.AddWalletAsync(wallet1);

			// get our wallet address
			var address1 = wallet1.GetAddressByIndex(1).ToHex();

			// now let's mine some coins (by creating a new block with our reward transaction in it)
			var newBlock = await _miner.Value.MineAsync(address1);

			// time to cash in - what's our balance now?
			var balance1 = await _operator.Value.GetBalanceForWalletAddressAsync(wallet1.Id, address1);

			// oh, we forgot to add the block to the block chain
			await _blockchain.Value.AddBlockAsync(newBlock);

			// let's look again
			balance1 = await _operator.Value.GetBalanceForWalletAddressAsync(wallet1.Id, address1);

			// now let's make another wallet and address to send our coins to
			var wallet2 = _operator.Value.CreateWalletFromPassword("i'm feeling generous today");
			await _operator.Value.AddWalletAsync(wallet2);
			var address2 = wallet2.GetAddressByIndex(1).ToHex();

			// create and sign a new transaction to ourselves (we're not a charity!)
			var transaction = await _operator.Value.CreateTransactionAsync(wallet1.Id, fromAddress: address1.FromHex(),
				toAddress: address2.FromHex(), amount: 5000000000L);
			transaction = await _blockchain.Value.AddTransactionAsync(transaction);

			// time to cash in - what's our balance now?
			balance1 = await _operator.Value.GetBalanceForWalletAddressAsync(wallet1.Id, address1);

			// oh, right... our transaction is only pending, back to work!
			newBlock = await _miner.Value.MineAsync(address1);
			await _blockchain.Value.AddBlockAsync(newBlock);

			// FIN.
			balance1 = await _operator.Value.GetBalanceForWalletAddressAsync(wallet1.Id, address1);
			var balance2 = await _operator.Value.GetBalanceForWalletAddressAsync(wallet2.Id, address2);

			Console.WriteLine("FIN.");
		}
	}
}
