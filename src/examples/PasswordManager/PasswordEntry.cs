using ChainLib.Models;
using ChainLib.Serialization;

namespace PasswordManager
{
	/// <summary>
	/// A wallet is the beginning of user participation in a blockchain.
	/// When we have a place to place protected assets, storage is the only feature we need for many applications.
	/// An ideal use is a password manager, where we store multiple passwords identified by a key (where the password is for)
	/// and the password hashes themselves are protected by our private key.
	/// 
	/// We can add features such as detecting when a password is pwned, automatically generating new passwords on a schedule, etc.
	/// </summary>
	public class PasswordEntry : IBlockSerialized
    {
		public byte[] Address { get; set; }
		public byte[] Identifier { get; set; }
		public byte[] Password { get; set; }
		public byte[] PasswordHash { get; set; }
		
	    public PasswordEntry(BlockDeserializeContext context)
	    {
		    Address = context.br.ReadBuffer();
		    Identifier = context.br.ReadBuffer();
		    Password = context.br.ReadBuffer();
		    PasswordHash = context.br.ReadBuffer();
		}

		public void Serialize(BlockSerializeContext context)
	    {
		    context.bw.WriteBuffer(Address);
			context.bw.WriteBuffer(Identifier);
		    context.bw.WriteBuffer(Password);
		    context.bw.WriteBuffer(PasswordHash);
		}
    }

	/// <summary>
	/// Next, we need to make a fundamental decision about how we can add new blocks to our chain.
	/// 
	/// For now, we'll simply allow anyone to add a block at any time, provided that every block object (i.e. our passwords) 
	/// belong to the "miner" who is adding the block. 
	/// 
	/// Basically we want this to be a free application, but we don't want someone to be able to create fake passwords on our
	/// own account, rendering it unusable.
	/// 
	/// Later, we'll have to address another issue of a bad actor filling the network with bogus data to bring down the chain.
	/// </summary>
	public class NoProofOfWork : IProofOfWork
	{
		public double GetDifficulty(long index)
		{
			return 0;
		}

		public Block ProveWorkFor(Block block, double difficulty)
		{
			return block;
		}
	}
}