using System.Security.Cryptography;

namespace Hammock.Mono.Security.Cryptography
{
  public static class KeyBuilder
  {
    private static RandomNumberGenerator rng;

    private static RandomNumberGenerator Rng
    {
      get { return rng ?? (rng = RandomNumberGenerator.Create()); }
    }

    public static byte[] Key(int size)
    {
      var data = new byte[size];
      Rng.GetBytes(data);
      return data;
    }

    public static byte[] IV(int size)
    {
      var data = new byte[size];
      Rng.GetBytes(data);
      return data;
    }
  }
}