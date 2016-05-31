using System;
using System.Security.Cryptography;
using Hammock.Mono.Security.Cryptography;

namespace Hammock.Security.Cryptography
{
  public class HMACSHA1 : KeyedHashAlgorithm
  {
    private readonly HMACAlgorithm hmac;
    private bool m_disposed;

    public override sealed byte[] Key
    {
      get
      {
        return base.Key;
      }
      set
      {
        this.hmac.Key = value;
        base.Key = value;
      }
    }

    public string HashName
    {
      get
      {
        return this.hmac.HashName;
      }
      set
      {
        if (this.State == 0)
          this.hmac.HashName = value;
      }
    }

    public HMACSHA1()
      : this(KeyBuilder.Key(8))
    {
    }

    public HMACSHA1(byte[] rgbKey)
    {
      this.hmac = new HMACAlgorithm("SHA1");
      this.HashSizeValue = 160;
      this.Key = rgbKey;
      this.m_disposed = false;
    }

    ~HMACSHA1()
    {
      this.Dispose(false);
    }

    protected override void Dispose(bool disposing)
    {
      if (!this.m_disposed)
      {
        if (this.hmac != null)
          this.hmac.Dispose();
        base.Dispose(disposing);
        this.m_disposed = true;
      }
    }

    public override void Initialize()
    {
      if (this.m_disposed)
        throw new ObjectDisposedException("HMACSHA1");
      else if (!(this.hmac.Algo is SHA1))
      {
        throw new InvalidCastException(string.Format("Invalid hash algorithm '{0}', expected '{1}'.", this.hmac.Algo == null ? (object)"none" : (object)this.hmac.Algo.GetType().ToString(), (object)"SHA1"));
      }
      else
      {
        this.State = 0;
        this.hmac.Initialize();
      }
    }

    protected override void HashCore(byte[] rgb, int ib, int cb)
    {
      if (this.m_disposed)
      {
        throw new ObjectDisposedException("HMACSHA1");
      }
      else
      {
        if (this.State == 0)
        {
          this.Initialize();
          this.State = 1;
        }
        this.hmac.Core(rgb, ib, cb);
      }
    }

    protected override byte[] HashFinal()
    {
      if (this.m_disposed)
      {
        throw new ObjectDisposedException("HMACSHA1");
      }
      else
      {
        this.State = 0;
        return this.hmac.Final();
      }
    }
  }
}