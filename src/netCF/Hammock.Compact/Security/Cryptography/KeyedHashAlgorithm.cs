using System;
using System.Security.Cryptography;

namespace Hammock.Security.Cryptography
{
  public abstract class KeyedHashAlgorithm : HashAlgorithm
  {
    protected byte[] KeyValue;

    public virtual byte[] Key
    {
      get
      {
        return (byte[])this.KeyValue.Clone();
      }
      set
      {
        if (this.State != 0)
        {
          throw new CryptographicException("Key can't be changed at this state.");
        }

        this.ZeroizeKey();
        this.KeyValue = (byte[])value.Clone();
      }
    }

    ~KeyedHashAlgorithm()
    {
      this.Dispose(false);
    }

    protected override void Dispose(bool disposing)
    {
      this.ZeroizeKey();
      base.Dispose(disposing);
    }

    private void ZeroizeKey()
    {
      if (this.KeyValue != null)
        Array.Clear(this.KeyValue, 0, this.KeyValue.Length);
    }

    public static new KeyedHashAlgorithm Create()
    {
      return Create("System.Security.Cryptography.KeyedHashAlgorithm");
    }

    public static new KeyedHashAlgorithm Create(string algName)
    {
      return (KeyedHashAlgorithm)CryptoConfig.CreateFromName(algName);
    }
  }
}