using System;
using System.Security.Cryptography;

namespace Hammock.Mono.Security.Cryptography
{
  public class BlockProcessor
  {
    private ICryptoTransform transform;
    private byte[] block;
    private int blockSize;
    private int blockCount;

    public BlockProcessor(ICryptoTransform transform)
      : this(transform, transform.InputBlockSize)
    {
    }

    public BlockProcessor(ICryptoTransform transform, int blockSize)
    {
      this.transform = transform;
      this.blockSize = blockSize;
      this.block = new byte[blockSize];
    }

    ~BlockProcessor()
    {
      Array.Clear((Array)this.block, 0, this.blockSize);
    }

    public void Initialize()
    {
      Array.Clear((Array)this.block, 0, this.blockSize);
      this.blockCount = 0;
    }

    public void Core(byte[] rgb)
    {
      this.Core(rgb, 0, rgb.Length);
    }

    public void Core(byte[] rgb, int ib, int cb)
    {
      int count = Math.Min(this.blockSize - this.blockCount, cb);
      Buffer.BlockCopy((Array)rgb, ib, (Array)this.block, this.blockCount, count);
      this.blockCount += count;
      if (this.blockCount == this.blockSize)
      {
        this.transform.TransformBlock(this.block, 0, this.blockSize, this.block, 0);
        int num = (cb - count) / this.blockSize;
        for (int index = 0; index < num; ++index)
        {
          this.transform.TransformBlock(rgb, count + ib, this.blockSize, this.block, 0);
          count += this.blockSize;
        }
        this.blockCount = cb - count;
        if (this.blockCount > 0)
          Buffer.BlockCopy((Array)rgb, count + ib, (Array)this.block, 0, this.blockCount);
      }
    }

    public byte[] Final()
    {
      return this.transform.TransformFinalBlock(this.block, 0, this.blockCount);
    }
  }
}