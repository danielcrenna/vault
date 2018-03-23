using System.Collections.Generic;
using System.Linq;
using ChainLib.Crypto;
using ChainLib.Models;

namespace ChainLib.Extensions
{
	public static class BlockExtensions
	{
		public static string ToHashString(this Block block, IHashProvider hashProvider)
		{
			return hashProvider.ComputeHashString(block);
		}

		public static byte[] ToHashBytes(this Block block, IHashProvider hashProvider)
		{
			foreach (var @object in block?.Objects ?? Enumerable.Empty<BlockObject>())
			{
				@object.Hash = hashProvider.ComputeHashBytes(@object);
			}

			return hashProvider.ComputeHashBytes(block);
		}

		public static byte[] ToHashBytes(this IBlockSerialized data, IHashProvider hashProvider)
		{
			return hashProvider.ComputeHashBytes(data);
		}
		
		public static byte[] ComputeMerkleRoot(this Block block, IHashProvider hashProvider)
		{
			if (block.Objects == null || block.Objects.Count == 0)
				return hashProvider.ComputeHashBytes(Block.NoObjects);

			// https://en.bitcoin.it/wiki/Protocol_documentation#Merkle_Trees

			var p = new List<byte[]>();
			foreach (var o in block.Objects)
				p.Add(hashProvider.DoubleHash(o));
			if(p.Count > 1 && p.Count % 2 != 0)
				p.Add(p[p.Count - 1]);
			if (p.Count == 1)
				return p[0];

			pass:
			{
				var n = new List<byte[]>(p.Count / 2);
				for (var i = 0; i < p.Count; i++)
				{
					for (var j = i + 1; j < p.Count; j++)
					{
						var a = block.Objects[i].Hash;
						var b = block.Objects[j].Hash;
						var d = hashProvider.DoubleHash(a.ConcatArrays(b));
						n.Add(d);
						i++;
					}
				}
				if (n.Count == 1)
					return n[0];
				if (n.Count % 2 != 0)
					n.Add(n[n.Count - 1]);

				p = n;
				goto pass;
			}
		}
	}
}