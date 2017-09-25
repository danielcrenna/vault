using System;
using System.Runtime.InteropServices;

namespace Sodium.Interop
{
  public class SodiumRuntimeConfig
  {
    internal const string LibraryName = "libsodium";

    internal static readonly bool IsRunningOnMono
      = Type.GetType("Mono.Runtime") != null;

#pragma warning disable 414
    internal static readonly string ArchitectureDirectory;
#pragma warning restore 414

    static SodiumRuntimeConfig()
    {
      switch (RuntimeInformation.ProcessArchitecture)
      {
        case Architecture.X86:
          ArchitectureDirectory = "x86";
          break;
        case Architecture.X64:
          ArchitectureDirectory = "x64";
          break;
        case Architecture.Arm:
          ArchitectureDirectory = "arm";
          break;
        case Architecture.Arm64:
          ArchitectureDirectory = "arm64";
          break;

        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}