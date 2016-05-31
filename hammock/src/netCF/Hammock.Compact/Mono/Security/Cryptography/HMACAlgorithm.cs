using System;
using System.Security.Cryptography;

namespace Hammock.Mono.Security.Cryptography
{
  internal class HMACAlgorithm
  {
    private HashAlgorithm algo;
    private BlockProcessor block;
    private byte[] hash;
    private string hashName;
    private byte[] key;

    public HashAlgorithm Algo
    {
      get
      {
        return this.algo;
      }
    }

    public string HashName
    {
      get
      {
        return this.hashName;
      }
      set
      {
        this.CreateHash(value);
      }
    }

    public byte[] Key
    {
      get
      {
        return this.key;
      }
      set
      {
        if (value != null && value.Length > 64)
          this.key = this.algo.ComputeHash(value);
        else if (value != null)
          this.key = (byte[])value.Clone();
      }
    }

    public HMACAlgorithm(string algoName)
    {
      this.CreateHash(algoName);
    }

    ~HMACAlgorithm()
    {
      this.Dispose();
    }

    private void CreateHash(string algoName)
    {
      this.algo = HashAlgorithm.Create(algoName);
      this.hashName = algoName;
      this.block = new BlockProcessor((ICryptoTransform)this.algo, 8);
    }

    public void Dispose()
    {
      if (this.key != null)
        Array.Clear((Array)this.key, 0, this.key.Length);
    }

    public void Initialize()
    {
      this.hash = (byte[])null;
      this.block.Initialize();
      byte[] rgb = HMACAlgorithm.KeySetup(this.key, (byte)54);
      this.algo.Initialize();
      this.block.Core(rgb);
      Array.Clear((Array)rgb, 0, rgb.Length);
    }

    private static byte[] KeySetup(byte[] key, byte padding)
    {
      byte[] numArray = new byte[64];
      for (int index = 0; index < key.Length; ++index)
        numArray[index] = (byte)((uint)key[index] ^ (uint)padding);
      for (int length = key.Length; length < 64; ++length)
        numArray[length] = padding;
      return numArray;
    }

    public void Core(byte[] rgb, int ib, int cb)
    {
      this.block.Core(rgb, ib, cb);
    }

    public byte[] Final()
    {
      this.block.Final();
      byte[] hash = this.algo.Hash;
      byte[] numArray = HMACAlgorithm.KeySetup(this.key, (byte)92);
      this.algo.Initialize();
      this.algo.TransformBlock(numArray, 0, numArray.Length, numArray, 0);
      this.algo.TransformFinalBlock(hash, 0, hash.Length);
      this.hash = this.algo.Hash;
      Array.Clear((Array)numArray, 0, numArray.Length);
      Array.Clear((Array)hash, 0, hash.Length);
      return this.hash;
    }
  }
}